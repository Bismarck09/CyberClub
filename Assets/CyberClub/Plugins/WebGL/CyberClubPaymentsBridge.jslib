mergeInto(LibraryManager.library, {
    CyberClub_RetryPaymentsCatalog_js: function () {
        if (typeof ysdk === 'undefined' || ysdk === null) {
            console.error('CyberClub payments: Yandex SDK is unavailable.');
            return 0;
        }

        if (typeof InitPayments !== 'function') {
            console.error('CyberClub payments: InitPayments is unavailable.');
            return 0;
        }

        try {
            InitPayments();
            return 1;
        } catch (error) {
            console.error('CyberClub payments: catalog retry failed.', error);
            return 0;
        }
    },

    CyberClub_BuyNonConsumable_js: function (productIdPointer) {
        var productId = UTF8ToString(productIdPointer);

        if (typeof payments === 'undefined' || payments === null) {
            console.error('CyberClub payments: payments object is unavailable.');
            return 0;
        }

        try {
            payments.purchase({ id: productId }).then(function () {
                LogStyledMessage('CyberClub non-consumable purchase success: ' + productId);
                YG2Instance('OnPurchaseSuccess', productId);
                FocusGame();
            }).catch(function (error) {
                console.error('CyberClub non-consumable purchase failed: ' + productId, error);
                YG2Instance('OnPurchaseFailed', productId);
                FocusGame();
            });

            return 1;
        } catch (error) {
            console.error('CyberClub payments: purchase bridge crashed.', error);
            YG2Instance('OnPurchaseFailed', productId);
            FocusGame();
            return 1;
        }
    }
});
