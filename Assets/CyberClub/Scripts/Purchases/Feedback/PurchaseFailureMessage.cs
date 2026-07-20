public static class PurchaseFailureMessage
{
    public static string Get(PurchaseFailureReason reason)
    {
        return reason switch
        {
            PurchaseFailureReason.NotEnoughCoins => "Недостаточно монет",
            PurchaseFailureReason.NotEnoughGems => "Недостаточно кристаллов",
            PurchaseFailureReason.MaximumReached => "Достигнут максимум",
            PurchaseFailureReason.FirstComputerRequired => "Сначала купи первый компьютер",
            PurchaseFailureReason.InteriorTutorialRequired => "Интерьер станет доступен во время обучения",
            PurchaseFailureReason.TutorialStageIncomplete => "Заверши текущий этап обучения",
            PurchaseFailureReason.LockedByTutorial => "Заверши текущий этап обучения",
            PurchaseFailureReason.ProductUnavailable => "Покупка сейчас недоступна",
            PurchaseFailureReason.TransactionFailed => "Не удалось завершить покупку",
            _ => string.Empty
        };
    }
}
