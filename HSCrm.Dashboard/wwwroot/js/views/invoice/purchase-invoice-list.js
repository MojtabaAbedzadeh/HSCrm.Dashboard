(function () {
    const apiAddress = window.AppContext?.apiAddress || '';
    const token = window.AppContext?.token || '';

    window.deletePurchaseInvoice = function (id) {
        // بررسی وجود کتابخانه swal
        if (typeof swal === 'undefined') {
            // fallback در صورت عدم لود کتابخانه
            if (confirm('آیا از حذف این فاکتور مطمئن هستید؟')) {
                executeDelete(id);
            }
            return;
        }

        // نمایش پیغام تاییدیه با sweetalert 1
        swal({
            title: "آیا مطمئن هستید؟",
            text: "این فاکتور برای همیشه حذف خواهد شد و قابل بازیابی نیست!",
            type: "warning", // در نسخه ۱ از type استفاده می‌شود
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "بله، حذف کن!",
            cancelButtonText: "انصراف",
            closeOnConfirm: false // باز نگه داشتن تا زمان اتمام درخواست api
        }, function (isConfirm) {
            if (isConfirm) {
                executeDelete(id);
            }
        });
    };

    // تابع کمکی برای انجام عملیات حذف
    async function executeDelete(id) {
        try {
            const response = await fetch(`${apiAddress}/PurchaseInvoice/Delete/${id}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    ...(token ? { 'Authorization': `Bearer ${token}` } : {})
                }
            });

            if (!response.ok) {
                throw new Error('خطا در ارتباط با سرور');
            }

            const result = await response.json();

            if (result.status) {
                if (typeof swal !== 'undefined') {
                    // نمایش پیغام موفقیت با sweetalert 1
                    swal({
                        title: "موفقیت‌آمیز",
                        text: "فاکتور با موفقیت حذف شد.",
                        type: "success"
                    }, function () {
                        location.reload();
                    });
                } else {
                    alert('فاکتور با موفقیت حذف شد');
                    location.reload();
                }
            } else {
                throw new Error(result.message || 'خطا در عملیات حذف');
            }
        } catch (error) {
            console.error('Error:', error);
            if (typeof swal !== 'undefined') {
                // نمایش پیغام خطا با sweetalert 1
                swal("خطا", error.message, "error");
            } else {
                alert(error.message);
            }
        }
    }
})();