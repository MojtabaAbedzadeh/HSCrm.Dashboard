let paymentModalInstance = null;

$(document).ready(function () {
    if (window.paymentApiBase) {
        loadPayments();
    }

    const modalEl = document.getElementById('paymentModal');
    if (modalEl) {
        paymentModalInstance = new bootstrap.Modal(modalEl);
    }
});

function authHeaders() {
    const token =
        localStorage.getItem('accessToken') ||
        sessionStorage.getItem('accessToken');

    const headers = {
        'Content-Type': 'application/json'
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    return headers;
}

function resetPaymentForm() {
    $('#PaymentId').val(0);
    $('#Amount').val('');
    $('#PaymentDate').val('');
    $('#Method').val('Cash');
    $('#ReferenceNo').val('');
    $('#SalesInvoiceId').val('');
    $('#PurchaseInvoiceId').val('');
    $('#ExpenseId').val('');
    $('#paymentModalTitle').text('ثبت پرداخت');
}

function loadPayments() {
    $.ajax({
        url: `${window.paymentApiBase}/GetAll`,
        method: 'GET',
        headers: authHeaders(),
        success: function (res) {
            const items = extractList(res);
            renderPayments(items);
        },
        error: function () {
            toastr?.error('خطا در دریافت لیست پرداخت‌ها');
        }
    });
}

function extractList(res) {
    if (Array.isArray(res)) return res;
    if (res?.data && Array.isArray(res.data)) return res.data;
    if (res?.result && Array.isArray(res.result)) return res.result;
    if (res?.value && Array.isArray(res.value)) return res.value;
    return [];
}

function renderPayments(items) {
    const tbody = $('#paymentTableBody');
    tbody.empty();

    if (!items || items.length === 0) {
        tbody.append(`
            <tr>
                <td colspan="6" class="text-center text-muted">اطلاعاتی یافت نشد</td>
            </tr>
        `);
        return;
    }

    items.forEach(item => {
        const dateText = formatDate(item.paymentDate || item.PaymentDate);
        const amount = item.amount ?? item.Amount ?? 0;
        const method = item.method ?? item.Method ?? '';
        const referenceNo = item.referenceNo ?? item.ReferenceNo ?? '';
        const salesInvoiceId = item.salesInvoiceId ?? item.SalesInvoiceId ?? '';
        const purchaseInvoiceId = item.purchaseInvoiceId ?? item.PurchaseInvoiceId ?? '';
        const expenseId = item.expenseId ?? item.ExpenseId ?? '';

        const refText =
            salesInvoiceId ? `Sales: ${salesInvoiceId}` :
                purchaseInvoiceId ? `Purchase: ${purchaseInvoiceId}` :
                    expenseId ? `Expense: ${expenseId}` :
                        '-';

        tbody.append(`
            <tr>
                <td>${numberWithCommas(amount)}</td>
                <td>${dateText}</td>
                <td>${method}</td>
                <td>${referenceNo || '-'}</td>
                <td>${refText}</td>
                <td>
                    <button type="button" class="btn btn-sm btn-warning"
                            onclick="showUpdateModal(${item.id ?? item.Id})">
                        ویرایش
                    </button>

                    <button type="button" class="btn btn-sm btn-danger"
                            onclick="deletePayment(${item.id ?? item.Id})">
                        حذف
                    </button>
                </td>
            </tr>
        `);
    });
}

function showUpdateModal(id) {
    $.ajax({
        url: `${window.paymentApiBase}/GetById?id=${id}`,
        method: 'GET',
        headers: authHeaders(),
        success: function (res) {
            const item = extractSingle(res);
            if (!item) {
                toastr?.error('پرداخت مورد نظر یافت نشد');
                return;
            }

            $('#PaymentId').val(item.id ?? item.Id ?? 0);
            $('#Amount').val(item.amount ?? item.Amount ?? 0);
            $('#PaymentDate').val(toInputDate(item.paymentDate ?? item.PaymentDate));
            $('#Method').val(item.method ?? item.Method ?? 'Cash');
            $('#ReferenceNo').val(item.referenceNo ?? item.ReferenceNo ?? '');
            $('#SalesInvoiceId').val(item.salesInvoiceId ?? item.SalesInvoiceId ?? '');
            $('#PurchaseInvoiceId').val(item.purchaseInvoiceId ?? item.PurchaseInvoiceId ?? '');
            $('#ExpenseId').val(item.expenseId ?? item.ExpenseId ?? '');

            $('#paymentModalTitle').text('ویرایش پرداخت');
            paymentModalInstance.show();
        },
        error: function () {
            toastr?.error('خطا در دریافت اطلاعات پرداخت');
        }
    });
}

function extractSingle(res) {
    if (!res) return null;
    if (res.data) return res.data;
    if (res.result) return res.result;
    if (res.value) return res.value;
    return res;
}

function savePayment() {
    const paymentId = parseInt($('#PaymentId').val() || '0', 10);
    const amount = parseFloat($('#Amount').val() || '0');
    const paymentDate = $('#PaymentDate').val();
    const method = $('#Method').val();
    const referenceNo = $('#ReferenceNo').val()?.trim() || null;

    const salesInvoiceId = parseNullableInt($('#SalesInvoiceId').val());
    const purchaseInvoiceId = parseNullableInt($('#PurchaseInvoiceId').val());
    const expenseId = parseNullableInt($('#ExpenseId').val());

    const refCount = [salesInvoiceId, purchaseInvoiceId, expenseId].filter(x => x !== null).length;

    if (amount <= 0) {
        toastr?.warning('مبلغ باید بزرگتر از صفر باشد');
        return;
    }

    if (!paymentDate) {
        toastr?.warning('تاریخ پرداخت را وارد کنید');
        return;
    }

    // مهم: دقیقاً یکی از این سه مرجع
    if (refCount !== 1) {
        toastr?.warning('فقط یکی از شناسه‌های فاکتور فروش، فاکتور خرید یا هزینه را وارد کنید');
        return;
    }

    const payload = {
        paymentId: paymentId,
        amount: amount,
        paymentDate: paymentDate,
        method: method,
        referenceNo: referenceNo,
        salesInvoiceId: salesInvoiceId,
        purchaseInvoiceId: purchaseInvoiceId,
        expenseId: expenseId
    };

    const isUpdate = paymentId > 0;
    const url = isUpdate
        ? `${window.paymentApiBase}/Update`
        : `${window.paymentApiBase}/Create`;

    $.ajax({
        url: url,
        method: isUpdate ? 'PUT' : 'POST',
        data: JSON.stringify(payload),
        headers: authHeaders(),
        success: function () {
            toastr?.success(isUpdate ? 'پرداخت ویرایش شد' : 'پرداخت ثبت شد');
            paymentModalInstance.hide();
            loadPayments();
        },
        error: function (xhr) {
            const msg = xhr?.responseJSON?.message || 'خطا در ذخیره پرداخت';
            toastr?.error(msg);
        }
    });
}

function deletePayment(id) {
    Swal.fire({
        title: 'حذف پرداخت',
        text: 'آیا از حذف این پرداخت مطمئن هستید؟',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'بله، حذف شود',
        cancelButtonText: 'انصراف'
    }).then((result) => {
        if (!result.isConfirmed) return;

        $.ajax({
            url: `${window.paymentApiBase}/Delete?id=${id}`,
            method: 'DELETE',
            headers: authHeaders(),
            success: function () {
                toastr?.success('پرداخت حذف شد');
                loadPayments();
            },
            error: function (xhr) {
                const msg = xhr?.responseJSON?.message || 'خطا در حذف پرداخت';
                toastr?.error(msg);
            }
        });
    });
}

function parseNullableInt(value) {
    if (value === null || value === undefined) return null;
    const trimmed = value.toString().trim();
    if (!trimmed) return null;

    const num = parseInt(trimmed, 10);
    return Number.isNaN(num) ? null : num;
}

function toInputDate(dateValue) {
    if (!dateValue) return '';
    const d = new Date(dateValue);
    if (isNaN(d.getTime())) return '';
    return d.toISOString().split('T')[0];
}

function formatDate(dateValue) {
    if (!dateValue) return '-';
    const d = new Date(dateValue);
    if (isNaN(d.getTime())) return '-';
    return d.toLocaleDateString('fa-IR');
}

function numberWithCommas(x) {
    if (x === null || x === undefined) return '0';
    return Number(x).toLocaleString('en-US');
}
