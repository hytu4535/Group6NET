using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Helpers;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Controllers;

[Authorize]
public class CommerceController(
    ICategoryService categoryService,
    ISupplierService supplierService,
    IProductService productService,
    IImportReceiptService importReceiptService,
    ICartService cartService,
    IOrderService orderService,
    IPaymentService paymentService,
    IInvoiceService invoiceService) : Controller
{
    // ===================== Categories =====================

    [Authorize(Policy = "categories.view")]
    public async Task<IActionResult> Categories(string? search, int page = 1)
    {
        SetNav("Quản lý danh mục", "Categories", "Commerce");
        return View(await categoryService.GetListAsync(search, page));
    }

    [Authorize(Policy = "categories.create")]
    [HttpGet]
    public IActionResult CreateCategory()
    {
        SetNav("Tạo danh mục", "Categories", "Commerce");
        return View(new CategoryUpsertViewModel());
    }

    [Authorize(Policy = "categories.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CategoryUpsertViewModel model)
    {
        SetNav("Tạo danh mục", "Categories", "Commerce");
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await categoryService.CreateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Tạo danh mục thành công";
        return RedirectToAction(nameof(Categories));
    }

    [Authorize(Policy = "categories.update")]
    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        SetNav("Cập nhật danh mục", "Categories", "Commerce");
        var model = await categoryService.GetForEditAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Policy = "categories.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(CategoryUpsertViewModel model)
    {
        SetNav("Cập nhật danh mục", "Categories", "Commerce");
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await categoryService.UpdateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Cập nhật danh mục thành công";
        return RedirectToAction(nameof(Categories));
    }

    [Authorize(Policy = "categories.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await categoryService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa danh mục" : result.Error;
        return RedirectToAction(nameof(Categories));
    }

    // ===================== Suppliers =====================

    [Authorize(Policy = "suppliers.view")]
    public async Task<IActionResult> Suppliers(string? search, int page = 1)
    {
        SetNav("Quản lý nhà cung cấp", "Suppliers", "Commerce");
        return View(await supplierService.GetListAsync(search, page));
    }

    [Authorize(Policy = "suppliers.create")]
    [HttpGet]
    public IActionResult CreateSupplier()
    {
        SetNav("Tạo nhà cung cấp", "Suppliers", "Commerce");
        return View(new SupplierUpsertViewModel());
    }

    [Authorize(Policy = "suppliers.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSupplier(SupplierUpsertViewModel model)
    {
        SetNav("Tạo nhà cung cấp", "Suppliers", "Commerce");
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await supplierService.CreateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Tạo nhà cung cấp thành công";
        return RedirectToAction(nameof(Suppliers));
    }

    [Authorize(Policy = "suppliers.update")]
    [HttpGet]
    public async Task<IActionResult> EditSupplier(int id)
    {
        SetNav("Cập nhật nhà cung cấp", "Suppliers", "Commerce");
        var model = await supplierService.GetForEditAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Policy = "suppliers.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSupplier(SupplierUpsertViewModel model)
    {
        SetNav("Cập nhật nhà cung cấp", "Suppliers", "Commerce");
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await supplierService.UpdateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Cập nhật nhà cung cấp thành công";
        return RedirectToAction(nameof(Suppliers));
    }

    [Authorize(Policy = "suppliers.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var result = await supplierService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa nhà cung cấp" : result.Error;
        return RedirectToAction(nameof(Suppliers));
    }

    // ===================== Products =====================

    [Authorize(Policy = "products.view")]
    public async Task<IActionResult> Products(string? search, int? categoryId, string? status, int page = 1)
    {
        SetNav("Quản lý sản phẩm", "Products", "Commerce");
        return View(await productService.GetListAsync(search, categoryId, status, page));
    }

    [Authorize(Policy = "products.create")]
    [HttpGet]
    public async Task<IActionResult> CreateProduct()
    {
        SetNav("Tạo sản phẩm", "Products", "Commerce");
        return View(await productService.BuildCreateModelAsync());
    }

    [Authorize(Policy = "products.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(ProductUpsertViewModel model)
    {
        SetNav("Tạo sản phẩm", "Products", "Commerce");
        await productService.PopulateOptionsAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await productService.CreateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Tạo sản phẩm thành công";
        return RedirectToAction(nameof(Products));
    }

    [Authorize(Policy = "products.update")]
    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        SetNav("Cập nhật sản phẩm", "Products", "Commerce");
        var model = await productService.GetForEditAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Policy = "products.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(ProductUpsertViewModel model)
    {
        SetNav("Cập nhật sản phẩm", "Products", "Commerce");
        await productService.PopulateOptionsAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await productService.UpdateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Cập nhật sản phẩm thành công";
        return RedirectToAction(nameof(Products));
    }

    [Authorize(Policy = "products.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleProductStatus(int id)
    {
        var result = await productService.ToggleStatusAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã cập nhật trạng thái sản phẩm" : result.Error;
        return RedirectToAction(nameof(Products));
    }

    [Authorize(Policy = "products.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await productService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa sản phẩm" : result.Error;
        return RedirectToAction(nameof(Products));
    }

    // ===================== Import Receipts =====================

    [Authorize(Policy = "import_receipts.view")]
    public async Task<IActionResult> ImportReceipts(string? search, int? supplierId, DateTime? from, DateTime? to, int page = 1)
    {
        SetNav("Quản lý phiếu nhập kho", "ImportReceipts", "Commerce");
        return View(await importReceiptService.GetListAsync(search, supplierId, from, to, page));
    }

    [Authorize(Policy = "import_receipts.create")]
    [HttpGet]
    public async Task<IActionResult> CreateImportReceipt()
    {
        SetNav("Tạo phiếu nhập kho", "ImportReceipts", "Commerce");
        return View(await importReceiptService.BuildCreateModelAsync());
    }

    [Authorize(Policy = "import_receipts.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateImportReceipt(ImportReceiptCreateViewModel model)
    {
        SetNav("Tạo phiếu nhập kho", "ImportReceipts", "Commerce");
        model = await importReceiptService.BuildCreateModelAsync(model);

        if (model.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Phiếu nhập cần có ít nhất một sản phẩm");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await importReceiptService.CreateAsync(model, User.GetUserId());
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Tạo phiếu nhập kho thành công";
        return RedirectToAction(nameof(ImportReceiptDetails), new { id = result.Data });
    }

    [Authorize(Policy = "import_receipts.view")]
    public async Task<IActionResult> ImportReceiptDetails(int id)
    {
        SetNav("Chi tiết phiếu nhập kho", "ImportReceipts", "Commerce");
        var model = await importReceiptService.GetDetailsAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Policy = "import_receipts.update")]
    [HttpGet]
    public async Task<IActionResult> EditImportReceipt(int id)
    {
        SetNav("Cập nhật phiếu nhập kho", "ImportReceipts", "Commerce");
        var model = await importReceiptService.GetForEditAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Policy = "import_receipts.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditImportReceipt(ImportReceiptEditViewModel model)
    {
        SetNav("Cập nhật phiếu nhập kho", "ImportReceipts", "Commerce");
        await importReceiptService.PopulateOptionsAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await importReceiptService.UpdateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Cập nhật phiếu nhập kho thành công";
        return RedirectToAction(nameof(ImportReceiptDetails), new { id = model.Id });
    }

    [Authorize(Policy = "import_receipts.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImportReceipt(int id)
    {
        var result = await importReceiptService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa phiếu nhập kho" : result.Error;
        return RedirectToAction(nameof(ImportReceipts));
    }

    [Authorize(Policy = "import_receipts.view")]
    public async Task<IActionResult> ImportDetails(int? receiptId, string? search, int page = 1)
    {
        SetNav("Chi tiết phiếu nhập", "ImportDetails", "Commerce");
        return View(await importReceiptService.GetDetailLinesAsync(receiptId, search, page));
    }

    // ===================== Carts =====================

    [Authorize(Policy = "carts.view")]
    public async Task<IActionResult> Carts(string? search, string? status, int page = 1)
    {
        SetNav("Quản lý giỏ hàng", "Carts", "Commerce");
        return View(await cartService.GetListAsync(search, status, page));
    }

    [Authorize(Policy = "carts.view")]
    public async Task<IActionResult> CartDetails(int id)
    {
        SetNav("Chi tiết giỏ hàng", "Carts", "Commerce");
        var model = await cartService.GetDetailsAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Policy = "carts.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCart(int id)
    {
        var result = await cartService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa giỏ hàng" : result.Error;
        return RedirectToAction(nameof(Carts));
    }

    [Authorize(Policy = "carts.view")]
    public async Task<IActionResult> CartItems(int? cartId, string? search, int page = 1)
    {
        SetNav("Chi tiết giỏ hàng", "CartItems", "Commerce");
        return View(await cartService.GetItemsAsync(cartId, search, page));
    }

    // ===================== Orders =====================

    [Authorize(Policy = "orders.view")]
    public async Task<IActionResult> Orders(string? search, string? status, DateTime? from, DateTime? to, int page = 1)
    {
        SetNav("Quản lý đơn hàng", "Orders", "Commerce");
        return View(await orderService.GetListAsync(search, status, from, to, page));
    }

    [Authorize(Policy = "orders.create")]
    [HttpGet]
    public async Task<IActionResult> CreateOrder()
    {
        SetNav("Tạo đơn hàng", "Orders", "Commerce");
        return View(await orderService.BuildCreateModelAsync());
    }

    [Authorize(Policy = "orders.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(OrderCreateViewModel model)
    {
        SetNav("Tạo đơn hàng", "Orders", "Commerce");
        model = await orderService.BuildCreateModelAsync(model);

        if (model.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Đơn hàng cần có ít nhất một sản phẩm");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await orderService.CreateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Tạo đơn hàng thành công";
        return RedirectToAction(nameof(OrderDetails), new { id = result.Data });
    }

    [Authorize(Policy = "orders.view")]
    public async Task<IActionResult> OrderDetails(int id)
    {
        SetNav("Chi tiết đơn hàng", "Orders", "Commerce");
        var model = await orderService.GetDetailsAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Policy = "orders.update")]
    [HttpGet]
    public async Task<IActionResult> EditOrder(int id)
    {
        SetNav("Cập nhật đơn hàng", "Orders", "Commerce");
        var model = await orderService.GetForEditAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Policy = "orders.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditOrder(OrderEditViewModel model)
    {
        SetNav("Cập nhật đơn hàng", "Orders", "Commerce");
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await orderService.UpdateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Cập nhật đơn hàng thành công";
        return RedirectToAction(nameof(OrderDetails), new { id = model.Id });
    }

    [Authorize(Policy = "orders.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeOrderStatus(int id, string status)
    {
        var result = await orderService.ChangeStatusAsync(id, status);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã cập nhật trạng thái đơn hàng" : result.Error;
        return RedirectToAction(nameof(OrderDetails), new { id });
    }

    [Authorize(Policy = "orders.view")]
    public async Task<IActionResult> OrderItems(int? orderId, string? search, int page = 1)
    {
        SetNav("Chi tiết đơn hàng", "OrderItems", "Commerce");
        return View(await orderService.GetItemsAsync(orderId, search, page));
    }

    // ===================== Payments =====================

    [Authorize(Policy = "payments.view")]
    public async Task<IActionResult> Payments(string? search, string? status, string? method, int page = 1)
    {
        SetNav("Quản lý thanh toán", "Payments", "Commerce");
        return View(await paymentService.GetListAsync(search, status, method, page));
    }

    [Authorize(Policy = "payments.create")]
    [HttpGet]
    public async Task<IActionResult> CreatePayment(int? orderId)
    {
        SetNav("Tạo giao dịch thanh toán", "Payments", "Commerce");
        return View(await paymentService.BuildCreateModelAsync(orderId));
    }

    [Authorize(Policy = "payments.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePayment(PaymentUpsertViewModel model)
    {
        SetNav("Tạo giao dịch thanh toán", "Payments", "Commerce");
        model = await paymentService.BuildCreateModelAsync(model: model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await paymentService.CreateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Tạo giao dịch thanh toán thành công";
        return RedirectToAction(nameof(Payments));
    }

    [Authorize(Policy = "payments.update")]
    [HttpGet]
    public async Task<IActionResult> EditPayment(int id)
    {
        SetNav("Cập nhật giao dịch thanh toán", "Payments", "Commerce");
        var model = await paymentService.GetForEditAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Policy = "payments.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPayment(PaymentUpsertViewModel model)
    {
        SetNav("Cập nhật giao dịch thanh toán", "Payments", "Commerce");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await paymentService.UpdateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Cập nhật giao dịch thanh toán thành công";
        return RedirectToAction(nameof(Payments));
    }

    [Authorize(Policy = "payments.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePayment(int id)
    {
        var result = await paymentService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa giao dịch thanh toán" : result.Error;
        return RedirectToAction(nameof(Payments));
    }

    // ===================== Invoices =====================

    [Authorize(Policy = "invoices.view")]
    public async Task<IActionResult> Invoices(string? search, DateTime? from, DateTime? to, int page = 1)
    {
        SetNav("Quản lý hóa đơn", "Invoices", "Commerce");
        return View(await invoiceService.GetListAsync(search, from, to, page));
    }

    [Authorize(Policy = "invoices.create")]
    [HttpGet]
    public async Task<IActionResult> CreateInvoice(int? orderId)
    {
        SetNav("Lập hóa đơn", "Invoices", "Commerce");
        return View(await invoiceService.BuildCreateModelAsync(orderId));
    }

    [Authorize(Policy = "invoices.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInvoice(InvoiceUpsertViewModel model)
    {
        SetNav("Lập hóa đơn", "Invoices", "Commerce");
        model = await invoiceService.BuildCreateModelAsync(model: model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await invoiceService.CreateAsync(model, User.GetUserId());
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Lập hóa đơn thành công";
        return RedirectToAction(nameof(InvoiceDetails), new { id = result.Data });
    }

    [Authorize(Policy = "invoices.view")]
    public async Task<IActionResult> InvoiceDetails(int id)
    {
        SetNav("Chi tiết hóa đơn", "Invoices", "Commerce");
        var model = await invoiceService.GetDetailsAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Policy = "invoices.update")]
    [HttpGet]
    public async Task<IActionResult> EditInvoice(int id)
    {
        SetNav("Cập nhật hóa đơn", "Invoices", "Commerce");
        var model = await invoiceService.GetForEditAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Policy = "invoices.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditInvoice(InvoiceUpsertViewModel model)
    {
        SetNav("Cập nhật hóa đơn", "Invoices", "Commerce");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await invoiceService.UpdateAsync(model);
        if (!result.Success)
        {
            AddError(result);
            return View(model);
        }

        TempData["Success"] = "Cập nhật hóa đơn thành công";
        return RedirectToAction(nameof(InvoiceDetails), new { id = model.Id });
    }

    // ===================== Helpers =====================

    private void AddError(ServiceResult result)
    {
        if (!string.IsNullOrEmpty(result.Field))
        {
            ModelState.AddModelError(result.Field, result.Error ?? "Dữ liệu không hợp lệ");
        }
        else
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Dữ liệu không hợp lệ");
        }
    }

    private void SetNav(string title, string active, string activeParent)
    {
        ViewData["Title"] = title;
        ViewData["Active"] = active;
        ViewData["ActiveParent"] = activeParent;
    }
}
