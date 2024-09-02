using UnityEngine;
using System;
using TMPro;

namespace Presentation
{
    public class TimeoutView : MonoBehaviour
    {
		[SerializeField] private TextMeshProUGUI timeoutText = null;
		[SerializeField] private bool remainingMode = true;
		[SerializeField] private bool simpleViewMode = false;
		[SerializeField] private bool showHoursInSimpleViewMode = false;
		[SerializeField] private bool showMinutesInSimpleViewMode = false;
		
		[SerializeField] private string template = string.Empty;
		[SerializeField] private string hiddenText = "---";
		[SerializeField] private string timedOutTemplate = "{0} ago";

		private float time;
		private TimeSpan timeSpan = new TimeSpan(); 
		private bool visible = false;
		private bool started = false;

		private void Update()
		{
            if (visible && started)
            {
                float dt = remainingMode ? time - Time.timeSinceLevelLoad : Time.timeSinceLevelLoad - time;
                int totalSeconds = Mathf.CeilToInt(dt);
                int tt = Mathf.CeilToInt((float) timeSpan.TotalSeconds);

                if (totalSeconds != tt
                    || totalSeconds > 0 && tt < 0
                    || totalSeconds < 0 && tt > 0)
                {
	                UpdateLabel(dt);
                }
            }
		}

        private void UpdateLabel(float dt)
        {
	        UpdateLabel(TimeSpan.FromSeconds(dt));
        }

		private void UpdateLabel(TimeSpan timeSpan)
		{
			this.timeSpan = timeSpan;

			string value;

            if (!visible)
			{
			    value = hiddenText;
			}
			else
			{
				if (timeSpan.TotalSeconds > 0)
			    {
			        value = !string.IsNullOrEmpty(template) && template.Contains("{0}")
			            ? string.Format(template, TimeSpanToString(timeSpan))
			            : TimeSpanToString(timeSpan);
			    }
			    else
			    {
			        value = !string.IsNullOrEmpty(timedOutTemplate) && timedOutTemplate.Contains("{0}")
			            ? string.Format(timedOutTemplate, TimeSpanToString(timeSpan.Duration()))
			            : timedOutTemplate;
			    }
			}

            if (timeoutText != null)
	            timeoutText.text = value;
		}

		private string TimeSpanToString(TimeSpan timeLeft)
		{
			if (!simpleViewMode)
				return TimeSpanToExtendedString(timeLeft.Duration());

		    return TimeSpanToSimpleString(timeLeft.Duration(), showHoursInSimpleViewMode, showMinutesInSimpleViewMode);
		}

        private void SetTime(float time)
        {
	        visible = true;
	        
	        this.time = time;
            var dt = remainingMode ? time - Time.timeSinceLevelLoad : Time.timeSinceLevelLoad - time;
            
			UpdateLabel(dt);
        }
        
        public void SetTimeout(float timeout)
        {
	        SetTime(Time.timeSinceLevelLoad + timeout);
        }

        public void StartTimeout()
        {
	        visible = true;
	        started = true;
        }
        
        public void StopTimeout()
        {
	        started = false;
        }

        public void Hide()
		{
			visible = false;
			started = false;
			UpdateLabel(timeSpan);
		}
		
		public static string TimeSpanToExtendedString(TimeSpan timeLeft)
        {
			const string YearsTimeFormat = "{0} days";
			const string DaysTimeFormat = "{0} days {1} h";// {2:00} min";
			const string HoursRoundTimeFormat = "{0} h";
			const string HoursTimeFormat = "{0} h {1:00} min";//" {2:00} sec";
			const string MinsTimeFormat = "{0} min {1:00} sec";
			const string SecsTimeFormat = "{0} sec";
	        
			timeLeft = timeLeft.Duration();
			int sec = Mathf.CeilToInt(timeLeft.Seconds + timeLeft.Milliseconds / 1e3f);
            int mins = timeLeft.Minutes;
            int hours = timeLeft.Hours;
            int days = timeLeft.Days;
			//int months = timeLeft.Days;

            if (sec == 60 && mins > 0)
            {
                mins++;
                sec = 0;
            }

            if (mins == 60 && hours > 0)
            {
                hours++;
                mins = 0;
            }

            if (hours == 24 && days > 0)
            {
                days++;
                hours = 0;
            }

            string txt = String.Empty;
			if (days > 7 || days > 1 && hours <= 0)
				txt = String.Format(days > 1 ? YearsTimeFormat : YearsTimeFormat.Replace("days", "day"), days);
			//else if (days > 0)
			//	txt = String.Format(days > 1 ? MonthsTimeFormat : MonthsTimeFormat.Replace("days", "day"), days, hours);
            else if (days == 1 && hours <= 0)
                txt = String.Format(HoursTimeFormat, 24, mins);
            else if (days > 0)
                txt = String.Format(days > 1 ? DaysTimeFormat : DaysTimeFormat.Replace("days", "day"), days, hours);//, mins);
            else if (hours > 0 && mins <= 0)
                txt = String.Format(HoursRoundTimeFormat, hours);
            else if (hours > 0)
                txt = String.Format(HoursTimeFormat, hours, mins);//, sec);
            else if (mins > 0)
                txt = String.Format(MinsTimeFormat, mins, sec);
            else
                txt = String.Format(SecsTimeFormat, sec);

            return txt;
        }

        public static string TimeSpanToSimpleString(TimeSpan timeLeft, bool alwaysShowHours, bool alwaysShowMinutes)
        {
            int sec = Mathf.CeilToInt(timeLeft.Seconds + timeLeft.Milliseconds / 1e3f);
            int mins = Mathf.FloorToInt(timeLeft.Minutes);

            if (sec == 60)
            {
                sec = 0;
                mins++;
            }

            double hours = Math.Floor(timeLeft.TotalHours);

            if (mins == 60)
            {
                mins = 0;
                hours++;
            }

            if (alwaysShowHours || hours > 0)
                return string.Format("{0:00}:{1:00}:{2:00}", hours, mins, sec);
            
            if (alwaysShowMinutes || mins > 0)
	            return string.Format("{0:00}:{1:00}", mins, sec);
            
            return string.Format("{0:00}", sec);
        }
    }
}