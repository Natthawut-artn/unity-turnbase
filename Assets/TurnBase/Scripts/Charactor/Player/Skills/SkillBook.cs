using System.Collections.Generic;
using UnityEngine;

public class SkillBook : MonoBehaviour
{
    public List<Skill> skillsSet = new List<Skill>();
    void Awake()
    {
        skillsSet.Add(new CarnageSlice());
    }
}