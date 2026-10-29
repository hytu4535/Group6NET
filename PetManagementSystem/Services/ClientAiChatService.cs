using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;

namespace PetManagementSystem.Services;

public class ClientAiChatService(
    IChatConversationRepository chatConversationRepository,
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<ClientAiChatService> logger) : IClientAiChatService
{
    private const string ChatCompletionsUrl = "https://api.openai.com/v1/chat/completions";
    private const string FallbackReply =
        "Mình chưa có đủ thông tin để trả lời chính xác câu hỏi này. Bạn có thể liên hệ đội ngũ PetCare tại trang Liên hệ để được hỗ trợ nhé.";

    public Task<PagedResult<ChatConversation>> GetHistoryAsync(int userId, int page, int pageSize)
    {
        return chatConversationRepository.GetByUserIdPagedAsync(userId, page, pageSize);
    }

    public async Task<ServiceResult<string>> SendMessageAsync(
        int userId, string message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return ServiceResult<string>.Fail("Vui lòng nhập nội dung tin nhắn.", "message");
        }

        var question = message.Trim();
        var reply = GetFaqReply(question);
        if (reply is null)
        {
            var apiKey = configuration["AiChat:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                reply = FallbackReply;
            }
            else
            {
                var aiResult = await GetAiReplyAsync(question, apiKey, cancellationToken);
                if (!aiResult.Success)
                {
                    return aiResult;
                }

                reply = aiResult.Data!;
            }
        }

        await chatConversationRepository.AddAsync(new ChatConversation
        {
            UserId = userId,
            Title = question[..Math.Min(question.Length, 255)],
            UserRequest = question,
            AiResponse = reply,
            CreatedAt = DateTime.UtcNow
        });

        return ServiceResult<string>.Ok(reply);
    }

    private async Task<ServiceResult<string>> GetAiReplyAsync(
        string question, string apiKey, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model = configuration["AiChat:Model"] ?? "gpt-4o-mini",
            max_tokens = 500,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = """
                        Bạn là trợ lý chăm sóc khách hàng của PetCare. Chỉ hướng dẫn khách tìm thông tin và sử dụng website.
                        Không bịa giá, dịch vụ, chính sách, giờ hoạt động hoặc thông tin đơn hàng vì bạn không được kết nối dữ liệu trực tiếp.
                        Với triệu chứng, bệnh, thuốc hoặc điều trị cho thú cưng, không chẩn đoán hay kê đơn; khuyên khách liên hệ bác sĩ thú y.
                        Không yêu cầu mật khẩu, mã xác thực hoặc thông tin thanh toán. Trả lời bằng tiếng Việt, ngắn gọn và lịch sự.
                        Nếu không chắc chắn, hãy nói rõ và hướng dẫn khách liên hệ PetCare.
                        """
                },
                new { role = "user", content = question }
            }
        });

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("PetCare AI request failed with status code {StatusCode}.",
                    (int)response.StatusCode);
                return ServiceResult<string>.Fail(
                    "Trợ lý AI hiện chưa thể trả lời. Vui lòng thử lại sau hoặc liên hệ PetCare.");
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
            var reply = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            if (string.IsNullOrWhiteSpace(reply))
            {
                logger.LogWarning("PetCare AI returned an empty response.");
                return ServiceResult<string>.Fail(
                    "Trợ lý AI chưa tạo được câu trả lời. Vui lòng thử lại hoặc liên hệ PetCare.");
            }

            return ServiceResult<string>.Ok(reply.Trim());
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "PetCare AI service could not be reached.");
            return ServiceResult<string>.Fail(
                "Trợ lý AI hiện chưa thể kết nối. Vui lòng thử lại sau hoặc liên hệ PetCare.");
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "PetCare AI request timed out.");
            return ServiceResult<string>.Fail(
                "Trợ lý AI phản hồi quá lâu. Vui lòng thử lại sau hoặc liên hệ PetCare.");
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "PetCare AI returned an invalid response.");
            return ServiceResult<string>.Fail(
                "Trợ lý AI trả về phản hồi không hợp lệ. Vui lòng thử lại sau hoặc liên hệ PetCare.");
        }
        catch (KeyNotFoundException exception)
        {
            logger.LogWarning(exception, "PetCare AI response did not contain a reply.");
            return ServiceResult<string>.Fail(
                "Trợ lý AI trả về phản hồi không hợp lệ. Vui lòng thử lại sau hoặc liên hệ PetCare.");
        }
        catch (IndexOutOfRangeException exception)
        {
            logger.LogWarning(exception, "PetCare AI response contained no choices.");
            return ServiceResult<string>.Fail(
                "Trợ lý AI chưa tạo được câu trả lời. Vui lòng thử lại sau hoặc liên hệ PetCare.");
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "PetCare AI response had an unexpected structure.");
            return ServiceResult<string>.Fail(
                "Trợ lý AI trả về phản hồi không hợp lệ. Vui lòng thử lại sau hoặc liên hệ PetCare.");
        }
    }

    private static string? GetFaqReply(string question)
    {
        var normalized = Normalize(question);
        if (ContainsAny(normalized, "benh", "trieu chung", "dau bung", "bo an", "khong an", "non",
                "tieu chay", "chay mau", "kho tho", "co giat", "thuoc", "dieu tri", "so cuu"))
        {
            return "Mình không thể chẩn đoán hoặc hướng dẫn dùng thuốc. Nếu thú cưng có dấu hiệu bất thường, bạn hãy liên hệ bác sĩ thú y để được tư vấn trực tiếp; trường hợp khẩn cấp, hãy đưa bé đến cơ sở thú y gần nhất.";
        }

        if (ContainsAny(normalized, "xin chao", "chao petcare", "hello", "ban la ai", "tro giup"))
        {
            return "Xin chào! Mình có thể hướng dẫn bạn tìm dịch vụ, sản phẩm, đặt lịch hoặc xem thông tin thú cưng trên PetCare. Bạn cần hỗ trợ nội dung nào?";
        }

        if (ContainsAny(normalized, "dat lich", "lich hen", "hen kham", "dat hen"))
        {
            return "Bạn có thể xem dịch vụ và tạo lịch hẹn tại mục Dịch vụ trên thanh điều hướng. Nếu cần hỗ trợ thêm, hãy vào trang Liên hệ.";
        }

        if (ContainsAny(normalized, "san pham", "mua hang", "mua do", "thuc an", "do choi"))
        {
            return "Bạn có thể xem các sản phẩm đang có tại mục Sản phẩm trên thanh điều hướng. Thông tin và giá sản phẩm được hiển thị tại trang đó.";
        }

        if (ContainsAny(normalized, "don hang", "tinh trang don", "theo doi don", "lich su mua"))
        {
            return "Bạn có thể xem thông tin đơn hàng trong mục Tài khoản > Đơn hàng của tôi. Nếu cần hỗ trợ với một đơn cụ thể, vui lòng liên hệ PetCare.";
        }

        if (ContainsAny(normalized, "thu cung cua toi", "ho so thu cung", "them thu cung", "thong tin thu cung"))
        {
            return "Bạn có thể xem và quản lý hồ sơ thú cưng tại mục Tài khoản > Thú cưng của tôi.";
        }

        if (ContainsAny(normalized, "dich vu", "goi dich vu", "spa", "tiem phong", "kham"))
        {
            return "Bạn có thể xem các dịch vụ hiện có tại mục Dịch vụ trên thanh điều hướng. Để hỏi thêm về dịch vụ phù hợp, hãy liên hệ đội ngũ PetCare.";
        }

        if (ContainsAny(normalized, "lien he", "so dien thoai", "gio mo cua", "dia chi", "nhan vien", "tu van"))
        {
            return "Bạn hãy mở mục Liên hệ trên thanh điều hướng để xem các kênh hỗ trợ chính thức của PetCare.";
        }

        return null;
    }

    private static bool ContainsAny(string text, params string[] phrases)
    {
        return phrases.Any(text.Contains);
    }

    private static string Normalize(string value)
    {
        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var withoutDiacritics = new string(decomposed
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .ToArray())
            .Replace('đ', 'd');
        return Regex.Replace(withoutDiacritics, @"[^a-z0-9\s]", " ")
            .ReplaceLineEndings(" ")
            .Trim();
    }
}
