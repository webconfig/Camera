using System.Timers.Timer;
public class TimeManager
{
    private static TimeManager Instance = new TimeManager();
    public event CallBack TimeAction;
    static TimeManager()
    {
    }

    public TimeManager()
    {
        System.Timers.Timer time = new System.Timers.Timer();
        time.Elapsed += time_Elapsed;
        time.AutoReset = true;
        time.Enabled = true;
    }

    void time_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if(TimeAction!=null)
        {
            TimeAction();
        }
    }

    public static TimeManager GetInstance()
    {
        return TimeManager.Instance;
    }
}
public delegate void CallBack();

