using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Services;

namespace PetManagementSystem.Controllers;

[Authorize]
public class FeedbacksController(IFeedbackService feedbackService) : Controller
{
    // GET: /Feedbacks (Danh sách toàn bộ Feedback trong hệ thống)
    public async Task<IActionResult> Index()
    {
        var feedbacks = await feedbackService.GetAllFeedbacksAsync();
        return View(feedbacks);
    }

    // POST: /Feedbacks/UpdateStatus (Admin đổi trạng thái 'pending' <-> 'resolved')
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var (success, errorMessage) = await feedbackService.UpdateStatusAsync(id, status);
        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Cập nhật trạng thái phản hồi thành công!";
        }

        return RedirectToAction(nameof(Index));
    }
}