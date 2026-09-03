// ================================
// Helpers
// ================================
const ApiAddress = window.AppContext.apiAddress || '';
const TId = window.AppContext.tenantId || '';
const UId = window.AppContext.userId || '';
var Token = window.AppContext.token || '';

$(document).ready(function () {
    $.ajaxSetup({
        beforeSend: function (xhr) {
            if (Token) {
                xhr.setRequestHeader("Authorization", `Bearer ${Token}`);
            }
        }
    });

    $('.persian-date').persianDatepicker({
        format: 'YYYY/MM/DD',
        autoClose: true,
        calendarSwitch: {
            enabled: false,
            format: "MMMM"
        }
    });
});

function formatPrice(value) {
    if (!value) return '0';

    return value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function unFormatPrice(value) {
    if (!value) return 0;

    return parseFloat(value.toString().replace(/,/g, '')) || 0;
}

function clearProductForm() {
    $('#ProductTitle').val('');
    $('#ProductId').val('');
    $('#productCount').val(0);
    $('#sellPrice').val('');
    $('#discount').val(0);
    $('#tax').val(0);
    $('#rowSum').val(0);
}

// ================================
// Product Search (Autocomplete)
// ================================
$('#ProductTitle').on('keyup', function () {
    const text = $(this).val().trim();

    if (text.length < 2) {
        $('#productAutoComplete').hide();
        return;
    }

    $.ajax({
        url: window.AppContext.apiAddress + 'Product/Search',
        type: 'GET',
        data: { term: text },
        headers: {
            'Authorization': 'Bearer ' + Token
        },
        success: function (res) {
            let html = '';

            res.forEach(p => {
                html += `
                    <li class="list-group-item product-item"
                        data-id="${p.id}"
                        data-title="${p.productTitle}"
                        data-sell="${p.sellPrice}">
                        ${p.productTitle} - ${formatPrice(p.sellPrice)}
                    </li>`;
            });

            $('#productAutoComplete').html(html).show();
        },
        error: function (xhr) {
            console.error("خطا در سرچ محصول:", xhr.status, xhr.responseText);
        }
    });
});

$(document).on('click', '.product-item', function () {
    $('#ProductTitle').val($(this).data('title'));
    $('#ProductId').val($(this).data('id'));
    $('#sellPrice').val(formatPrice($(this).data('sell')));
    $('#productAutoComplete').hide();
    $('#productCount').focus();
    $('#productCount').select();
});

// ================================
// Row Sum Calculation
// ================================
function calculateRowSum() {
    const qty = parseFloat($('#productCount').val()) || 0;
    const price = unFormatPrice($('#sellPrice').val());
    const discount = unFormatPrice($('#discount').val());
    const tax = unFormatPrice($('#tax').val());

    let sum = (qty * price) - discount + tax;

    if (sum < 0) {
        sum = 0;
    }

    $('#rowSum').val(formatPrice(sum));
}

$('#productCount, #sellPrice, #discount, #tax').on('keyup change', calculateRowSum);

// ================================
// Add Product
// ================================
$('#btnAddProduct').on('click', function () {
    const productId = $('#ProductId').val();
    const title = $('#ProductTitle').val();
    const qty = parseFloat($('#productCount').val()) || 0;

    if (!productId || qty <= 0) {
        swal.fire({
            title: "خطا",
            text: "محصول و تعداد را وارد کنید",
            icon: "error",
            confirmButtonColor: "green",
            confirmButtonText: "باشه",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });

        return;
    }

    const price = unFormatPrice($('#sellPrice').val());
    const discount = unFormatPrice($('#discount').val());
    const tax = unFormatPrice($('#tax').val());
    const sum = (qty * price) - discount + tax;

    let merged = false;

    $('#tblBody tr').each(function () {
        const rowProductId = $(this).find('td:eq(1)').text();

        if (rowProductId === productId) {
            const oldQty = parseFloat($(this).find('td:eq(3)').text());
            const newQty = oldQty + qty;
            const newSum = (newQty * price) - discount + tax;

            $(this).find('td:eq(3)').text(newQty);
            $(this).find('td:eq(7)').text(formatPrice(newSum));

            merged = true;
        }
    });

    if (!merged) {
        const rowIndex = $('#tblBody tr').length + 1;

        $('#tblBody').append(`
            <tr>
                <td hidden>${rowIndex}</td>
                <td hidden>${productId}</td>
                <td>${title}</td>
                <td style="text-align:center">${qty}</td>
                <td style="text-align:center">${formatPrice(price)}</td>
                <td style="text-align:center">${formatPrice(discount)}</td>
                <td style="text-align:center">${formatPrice(tax)}</td>
                <td style="text-align:center">${formatPrice(sum)}</td>
                <td style="text-align:center">
                    <button class="btn btn-sm btn-warning btnEdit">✎</button>
                    <button class="btn btn-sm btn-danger btnRemove">✖</button>
                </td>
            </tr>
        `);
    }

    clearProductForm();
    calculateInvoiceSum();
});

// ================================
// Edit Row
// ================================
$(document).on('click', '.btnEdit', function () {
    const row = $(this).closest('tr');

    $('#ProductId').val(row.find('td:eq(1)').text());
    $('#ProductTitle').val(row.find('td:eq(2)').text());
    $('#productCount').val(row.find('td:eq(3)').text());
    $('#sellPrice').val(row.find('td:eq(4)').text());
    $('#discount').val(row.find('td:eq(5)').text());
    $('#tax').val(row.find('td:eq(6)').text());
    $('#rowSum').val(row.find('td:eq(7)').text());

    row.remove();

    calculateInvoiceSum();
});

// ================================
// Remove Row
// ================================
$(document).on('click', '.btnRemove', function () {
    $(this).closest('tr').remove();

    calculateInvoiceSum();
});

// ================================
// Invoice Total
// ================================
function calculateInvoiceSum() {
    let total = 0;

    $('#tblBody tr').each(function () {
        total += unFormatPrice($(this).find('td:eq(7)').text());
    });

    total -= unFormatPrice($('#invoiceDiscount').val());
    total += unFormatPrice($('#invoiceTax').val());

    if (total < 0) {
        total = 0;
    }

    $('#invoiceSumPrice').val(formatPrice(total));
}

$('#invoiceDiscount, #invoiceTax').on('keyup change', calculateInvoiceSum);

// ================================
// Submit Invoice
// ================================
$('#btnRegInvoice').on('click', function () {
    if (!validateInvoiceForm()) {
        return;
    }

    const model = {
        projectId: parseInt($('#ProjectId').val()),
        warehouseId: parseInt($('#WarehouseId').val()),
        tenantId: TId,
        invoiceNumber: $('#invoiceNumber').val(),
        issueDate: $('#invoiceDate').val(),
        status: parseInt($('#InvoiceStatus').val()),
        invoiceDiscount: unFormatPrice($('#invoiceDiscount').val()),
        invoiceTax: unFormatPrice($('#invoiceTax').val()),
        sellerUserId: UId,
        items: []
    };

    $('#tblBody tr').each(function () {
        model.items.push({
            productId: parseInt($(this).find('td:eq(1)').text()),
            quantity: parseFloat($(this).find('td:eq(3)').text()),
            unitPrice: unFormatPrice($(this).find('td:eq(4)').text()),
            discount: unFormatPrice($(this).find('td:eq(5)').text()),
            tax: unFormatPrice($(this).find('td:eq(6)').text())
        });
    });

    $.ajax({
        url: ApiAddress + 'SalesInvoice/CreateSalesInvoice',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(model),

        success: function (status) {
            if (status.status === true) {
                swal.fire({
                    title: "موفق",
                    text: "فاکتور با موفقیت ثبت شد",
                    icon: "success",
                    confirmButtonColor: "green",
                    confirmButtonText: "باشه",
                    showConfirmButton: true,
                    timer: 1500,
                    allowOutsideClick: false
                });

                window.location.href = '/AdminArea/Invoice/SalesInvoice';

                return;
            }

            switch (status.statusCode) {
                case 204:
                    swal.fire({
                        title: "اطلاعات ناقص",
                        text: "آیتمی برای ثبت وجود ندارد",
                        icon: "warning",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        showConfirmButton: true,
                        timer: 1500,
                        allowOutsideClick: false
                    });
                    break;

                case 400:
                    swal.fire({
                        title: "درخواست نامعتبر",
                        text: "اطلاعات ارسالی صحیح نیست",
                        icon: "warning",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        showConfirmButton: true,
                        timer: 1500,
                        allowOutsideClick: false
                    });
                    break;

                case 403:
                    swal.fire({
                        title: "عدم دسترسی",
                        text: "شما مجاز به انجام این عملیات نیستید",
                        icon: "error",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        showConfirmButton: true,
                        timer: 1500,
                        allowOutsideClick: false
                    });
                    break;

                case 404:
                    swal.fire({
                        title: "یافت نشد",
                        text: "رکورد مورد نظر یافت نشد",
                        icon: "error",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        showConfirmButton: true,
                        timer: 1500,
                        allowOutsideClick: false
                    });
                    break;

                case 406:
                    swal.fire({
                        title: "کمبود موجودی",
                        text: "موجودی محصول کافی نمی‌باشد!",
                        icon: "warning",
                        confirmButtonColor: "green",
                        confirmButtonText: "باشه",
                        showConfirmButton: true,
                        timer: 1500,
                        allowOutsideClick: false
                    });
                    break;
            }
        },

        error: function () {
            swal.fire({
                title: "خطای ارتباط با سرور",
                text: "امکان ارتباط با سرور وجود ندارد، لطفاً مجدداً تلاش کنید",
                icon: "error",
                confirmButtonColor: "green",
                confirmButtonText: "باشه",
                showConfirmButton: true,
                timer: 1500,
                allowOutsideClick: false
            });
        }
    });
});

// ================================
// Validation
// ================================
function validateInvoiceForm() {
    if (!$('#InvoiceStatus').val()) {
        swal.fire({
            title: "خطا",
            text: "نوع فاکتور را انتخاب کنید",
            icon: "error",
            confirmButtonColor: "green",
            confirmButtonText: "باشه",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });

        return false;
    }

    if (!$('#ProjectId').val() || parseInt($('#ProjectId').val()) === 0) {
        swal.fire({
            title: "خطا",
            text: "پروژه را انتخاب کنید",
            icon: "error",
            confirmButtonColor: "green",
            confirmButtonText: "باشه",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });

        return false;
    }

    if (!$('#WarehouseId').val() || parseInt($('#WarehouseId').val()) === 0) {
        swal.fire({
            title: "خطا",
            text: "انبار را انتخاب کنید",
            icon: "error",
            confirmButtonColor: "green",
            confirmButtonText: "باشه",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });

        return false;
    }

    if (!$('#invoiceNumber').val().trim()) {
        swal.fire({
            title: "خطا",
            text: "شماره فاکتور را وارد کنید",
            icon: "error",
            confirmButtonColor: "green",
            confirmButtonText: "باشه",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });

        return false;
    }

    if (!$('#invoiceDate').val()) {
        swal.fire({
            title: "خطا",
            text: "تاریخ فاکتور را انتخاب کنید",
            icon: "error",
            confirmButtonColor: "green",
            confirmButtonText: "باشه",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });

        return false;
    }

    if ($('#tblBody tr').length === 0) {
        swal.fire({
            title: "خطا",
            text: "حداقل یک کالا باید به فاکتور اضافه شود",
            icon: "error",
            confirmButtonColor: "green",
            confirmButtonText: "باشه",
            showConfirmButton: true,
            timer: 1500,
            allowOutsideClick: false
        });

        return false;
    }

    return true;
}