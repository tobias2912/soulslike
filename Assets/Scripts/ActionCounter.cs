using System.Collections.Generic;
public class ActionCounter
{

    Dictionary<string, float> currentActions = new Dictionary<string, float>();

    public bool addAction(string action, float duration)
    {
        if (currentActions.ContainsKey(action))
        {
            return false;
        }
        currentActions.Add(action, duration);
        return true;
    }

    public bool isActionOngoing(string action)
    {
        return currentActions.ContainsKey(action);
    }

    //tick all current actions
    public void updateTime(float deltatime)
    {
        // List<string> ToRemove = new List<string>();
        Dictionary<string, float> updated = new Dictionary<string, float>();
        foreach (string name in currentActions.Keys)
        {
            float newTime = currentActions[name] - deltatime;
            if (newTime > 0f)
            {
                updated.Add(name, newTime);
            }
        }
        currentActions = updated;
    }


}