using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AssistantHintRuleData
{
    public string hintId;

    [TextArea]
    public string hintText;

    public List<string> requiredTags = new List<string>();
    public List<string> blockedTags = new List<string>();

    public int minAskedQuestions = 0;
    public bool showOnlyOnce = true;
}