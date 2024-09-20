using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _ClockApplication
{
    public class ClockView : MonoBehaviour
    {
        [field: SerializeField] public Transform SecondArrow { get; private set; }
        [field: SerializeField] public ClockArrow MinuteArrow { get; private set; }
        [field: SerializeField] public ClockArrow HourArrow { get; private set; }
        [field: SerializeField] public Text TimeText { get; private set; }
        [field: SerializeField] public Button EditButton { get; private set; }
        [field: SerializeField] public Button AcceptButton { get; private set; }
        [field: SerializeField] public Button CancelButton { get; private set; }
        [field: SerializeField] public InputField HoursInputField { get; private set; }
        [field: SerializeField] public InputField MinutesInputField { get; private set; }
        [SerializeField] private float _arrowMoveDuration = 0.1f;
        private readonly Vector3 _arrowMoveStep = new Vector3(0, 0, Clock.ARROW_STEP);
        private readonly Vector3 _arrowHourMoveStep = new Vector3(0, 0, Clock.ARROW_HOUR_STEP);
        public readonly UnityEvent<EArrowType, float> OnArrowMoved = new();

        private void Start()
        {
            EditButton.onClick.AddListener(StartEdit);
            CancelButton.onClick.AddListener(StopEdit);
            AcceptButton.onClick.AddListener(StopEdit);
            MinutesInputField.onValidateInput += ValidateInputField;
            HoursInputField.onValidateInput += ValidateInputField;
        }

        public void Initialize(DateTime dateTime)
        {
            SecondArrow.rotation = Quaternion.Euler(new Vector3(0, 0, -dateTime.Second * Clock.ARROW_STEP));
            float minuteAngle = dateTime.Minute * Clock.ARROW_STEP + (dateTime.Second / 60f) * Clock.ARROW_STEP;
            MinuteArrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -minuteAngle));
            float hourAngle = (dateTime.Hour % 12) * Clock.ARROW_HOUR_STEP + (dateTime.Minute / 60f) * Clock.ARROW_HOUR_STEP;
            HourArrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -hourAngle));
            SetTime(dateTime);
        }
        
        public void SetTime(DateTime dateTime)
        {
            TimeText.text = dateTime.ToString("HH:mm:ss");
        }

        public void SetInputTime(DateTime dateTime)
        {
            HoursInputField.text = dateTime.ToString("HH");
            MinutesInputField.text = dateTime.ToString("mm");
        }
        
        public void MoveArrows()
        {
            SecondArrow.DORotate(SecondArrow.eulerAngles - _arrowMoveStep, _arrowMoveDuration);
            MinuteArrow.transform.DORotate(MinuteArrow.transform.eulerAngles - _arrowMoveStep / 60, _arrowMoveDuration);
            HourArrow.transform.DORotate(HourArrow.transform.eulerAngles - (_arrowHourMoveStep / 60) / 60, _arrowMoveDuration);
        }
        
        private char ValidateInputField(string text, int charindex, char addedchar)
        {
            if (!int.TryParse(addedchar.ToString(), out _))
            {
                return '\0';
            }
            return addedchar;
        }
        
        private void ArrowDragEvent(ClockArrow arrow, PointerEventData eventData)
        {
            Vector3 relative = transform.InverseTransformPoint(eventData.position);
            float angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;
            float roundedAngle;
            if (arrow.Type is EArrowType.Hour)
            {
                roundedAngle = Mathf.Round(angle / Clock.ARROW_HOUR_STEP) * Clock.ARROW_HOUR_STEP;
            }
            else
            {
                roundedAngle = Mathf.Round(angle / Clock.ARROW_STEP) * Clock.ARROW_STEP;
            }
            arrow.transform.rotation = Quaternion.Euler(0, 0, -roundedAngle);
            OnArrowMoved?.Invoke(arrow.Type, arrow.transform.eulerAngles.z);
        }

        private void StartEdit()
        {
            TimeText.gameObject.SetActive(false);
            EditButton.gameObject.SetActive(false);
            AcceptButton.gameObject.SetActive(true);
            CancelButton.gameObject.SetActive(true);
            HoursInputField.gameObject.SetActive(true);
            MinutesInputField.gameObject.SetActive(true);
            HoursInputField.text = $"{TimeText.text[0]}{TimeText.text[1]}";
            MinutesInputField.text = $"{TimeText.text[3]}{TimeText.text[4]}";
            MinuteArrow.OnDragEvent.AddListener(ArrowDragEvent);
            HourArrow.OnDragEvent.AddListener(ArrowDragEvent);
        }

        private void StopEdit()
        {
            TimeText.gameObject.SetActive(true);
            EditButton.gameObject.SetActive(true);
            AcceptButton.gameObject.SetActive(false);
            CancelButton.gameObject.SetActive(false);
            HoursInputField.gameObject.SetActive(false);
            MinutesInputField.gameObject.SetActive(false);
            MinuteArrow.OnDragEvent.RemoveListener(ArrowDragEvent);
            HourArrow.OnDragEvent.RemoveListener(ArrowDragEvent);
        }
    }
}