using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace _ClockApplication
{
    public class Clock : MonoBehaviour
    {
        [SerializeField] private ClockView _clockView;
        [SerializeField] private string _timeUrl = "https://yandex.com/time/sync.json";
        public const float ARROW_STEP = 360 / 60f;
        public const float ARROW_HOUR_STEP = 360 / 12f;
        private readonly DateTime _startTime = new DateTime(2024, 1, 1, 0, 0, 0);
        private DateTime _dateTime;
        private DateTime _editDateTime;
        private WaitForSecondsRealtime _secondsWait;
        private Coroutine _timer;

        private void Start()
        {
            _dateTime = DateTime.Now;
            _secondsWait = new WaitForSecondsRealtime(1f);
            _clockView.OnArrowMoved.AddListener(ArrowMoved);
            _clockView.EditButton.onClick.AddListener(StartEdit);
            _clockView.AcceptButton.onClick.AddListener(SubmitTime);
            _clockView.CancelButton.onClick.AddListener(CancelEdit);
            StartCoroutine(RequestTime());
            _timer = StartCoroutine(Timer());
        }

        private IEnumerator Timer()
        {
            _secondsWait.Reset();
            while (true)
            {
                yield return _secondsWait;
                _dateTime = _dateTime.AddSeconds(1);
                _clockView.MoveArrows();
                _clockView.SetTime(_dateTime);
                if (_dateTime.Minute == 0 && _dateTime.Second == 0)
                {
                    break;
                }
            }
            yield return StartCoroutine(RequestTime());
            _timer = StartCoroutine(Timer());
        }

        private IEnumerator RequestTime()
        {
            using UnityWebRequest www = UnityWebRequest.Get(_timeUrl);
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
                _dateTime = DateTime.Now;
            }
            else
            {
                var timeData = JsonUtility.FromJson<TimeData>(www.downloadHandler.text);
                long ticks = long.Parse(timeData.time);
                _dateTime = _startTime.AddMilliseconds(ticks).ToLocalTime();
            }
            _clockView.Initialize(_dateTime);
        }

        private void ArrowMoved(EArrowType type, float angle)
        {
            if (angle < 0)
                angle += 360;
            int value = 0;
            switch (type)
            {
                case EArrowType.Second:
                    value = Mathf.FloorToInt((360 - angle) / 360 * 60);
                    if (value == 60)
                        value = 0;
                    _editDateTime = _editDateTime.AddSeconds(-_editDateTime.Second + value);
                    break;
                case EArrowType.Minute:
                    value = Mathf.FloorToInt((360 - angle) / 360 * 60);
                    if (value == 60)
                        value = 0;
                    _editDateTime = _editDateTime.AddMinutes(-_editDateTime.Minute + value);
                    break;
                case EArrowType.Hour:
                    value = Mathf.FloorToInt((360 - angle) / 360 * 12);
                    _editDateTime = _editDateTime.AddHours(-_editDateTime.Hour + value);
                    break;
            }
            _clockView.SetInputTime(_editDateTime);
        }

        private void StartEdit()
        {
            if(_timer == null)
                return;
            StopCoroutine(_timer);
            _editDateTime = _dateTime;
        }
        
        private void SubmitTime()
        {
            var time = $"{_clockView.HoursInputField.text}:{_clockView.MinutesInputField.text}:{_dateTime.Second}";
            _dateTime = _startTime.Add(TimeSpan.Parse(time));
            _clockView.Initialize(_dateTime);
            _timer = StartCoroutine(Timer());
        }
        
        private void CancelEdit()
        {
            StartCoroutine(RequestTime());
            _clockView.Initialize(_dateTime);
            _timer = StartCoroutine(Timer());
        }
    }
}
