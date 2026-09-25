using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Services;

namespace PetManagementSystem.Controllers;

[Authorize]
public class ChatConversationsController(IChatConversationService chatConversationService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var pagedConversations = await chatConversationService.GetPagedAsync(
            search, fromDate, toDate, page, pageSize);
        ViewData["Title"] = "Lịch sử chat";
        ViewData["Active"] = "Chats";
        ViewData["ActiveParent"] = "CustomerCare";
        ViewData["Search"] = search;
        ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
        ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
        ViewData["Page"] = pagedConversations.Page;
        ViewData["PageSize"] = pagedConversations.PageSize;
        ViewData["TotalItems"] = pagedConversations.TotalItems;
        return View("~/Views/Admin/Chats.cshtml", pagedConversations.Items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id, string? search, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
    {
        var deleted = await chatConversationService.DeleteAsync(id);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Đã xóa lịch sử chat."
            : "Không tìm thấy cuộc trò chuyện cần xóa.";
        return RedirectToAction(nameof(Index), new { search, fromDate, toDate, page, pageSize });
    }
}