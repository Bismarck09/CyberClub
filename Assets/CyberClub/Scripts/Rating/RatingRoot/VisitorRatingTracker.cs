using UnityEngine;

public class VisitorRatingTracker : MonoBehaviour
{
    private float _queueStartTime;
    private bool _isWaitingInQueue;
    private bool _isRated;

    public float CurrentWaitingTime => !_isWaitingInQueue ? 0f : Time.time - _queueStartTime;

    public void StartWaiting()
    {
        _queueStartTime = Time.time;
        _isWaitingInQueue = true;
        _isRated = false;
    }

    public void EvaluateWaitingTime(RatingData ratingData, float goodWaitTime, float positiveRatingChange, float negativeRatingChange, float extraPenaltyEverySeconds, float extraPenaltyAmount)
    {
        if (ratingData == null || _isRated)
            return;

        float waitingTime = CurrentWaitingTime;
        _isWaitingInQueue = false;
        _isRated = true;

        if (waitingTime <= goodWaitTime)
        {
            ratingData.AddRating(positiveRatingChange);
            Debug.Log($"Клиент доволен. Ожидание: {waitingTime:0.00} сек. Рейтинг +{positiveRatingChange:0.00}");
            return;
        }

        float penalty = negativeRatingChange;

        if (extraPenaltyEverySeconds > 0f && extraPenaltyAmount > 0f)
        {
            float extraWait = waitingTime - goodWaitTime;
            int extraSteps = Mathf.FloorToInt(extraWait / extraPenaltyEverySeconds);
            penalty += extraSteps * extraPenaltyAmount;
        }

        ratingData.RemoveRating(penalty);
        Debug.Log($"Клиент недоволен. Ожидание: {waitingTime:0.00} сек. Рейтинг -{penalty:0.00}");
    }
}
