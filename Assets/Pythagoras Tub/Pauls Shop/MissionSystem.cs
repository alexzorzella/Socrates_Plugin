using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionSystem : MonoBehaviour
{
    public List<Mission> missions = new List<Mission>();

    public int currentIndex = 0;

    public int foos;
    public int bars;

    bool completedAllMissions;

    private void Start()
    {
        missions.Add(new Mission(() => { return "Get three foos."; }, (object o) => { return foos >= 3; }));
        missions.Add(new Mission(() => { return "Have seven bars exactly."; }, (object o) => { return bars == 7; }));
    }

    private void Update()
    {

        if (AllMissionsCompleted() && !completedAllMissions)
        {
            completedAllMissions = true;
            Debug.Log($"Completed all");
            return;
        }

        foreach (var mission in missions)
        {
            mission.CheckForCompletion();
        }

        if (CurrentMission().completed)
        {
            Debug.Log($"Completed {CurrentMission().Description()}");
            currentIndex = IncrementWithOverflow.Run(currentIndex, missions.Count, 1);
        }
    }

    private Mission CurrentMission()
    {
        return missions[currentIndex];
    }

    bool AllMissionsCompleted()
    {
        foreach (var mission in missions)
        {
            if (!mission.completed)
            {
                return false;
            }
        }

        return true;
    }
}

public class Mission
{
    public Mission(Func<string> description, Predicate<object> predicate)
    {
        this.description = description;
        this.predicate = predicate;
    }

    public Func<string> description;
    public bool completed;
    public Predicate<object> predicate;

    public void CheckForCompletion()
    {
        if (completed) { return; }

        completed = Prerequisite();
    }

    public bool Prerequisite()
    {
        return predicate.Invoke(null);
    }

    internal string Description()
    {
        return description.Invoke();
    }
}