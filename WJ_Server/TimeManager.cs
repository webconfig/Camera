using System.Timers;
public class TimeManager
{
    private static TimeManager Instance = new TimeManager();
    public event CallBack TimeAction;
    static TimeManager()
    {
    }

    public TimeManager()
    {
        Timer time = new Timer();
        time.Elapsed += time_Elapsed;
        time.AutoReset = true;
        time.Enabled = true;
    }

    void time_Elapsed(object sender, ElapsedEventArgs e)
    {
        if(TimeAction!=null)
        {
            TimeAction();
        }
    }

    public static TimeManager GetInstance()
    {
        return Instance;
    }
}
public delegate void CallBack();

