using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ZooWorld.UI
{
    /// <summary>
    /// Passive view: one row per diet that is counted on screen. Counting another diet is a
    /// new row here and a new string in the table, not new code.
    /// </summary>
    public class DeathCounterView : MonoBehaviour
    {
        [SerializeField] private Row[] rows = Array.Empty<Row>();

        public IReadOnlyList<Row> Rows => rows;

        [Serializable]
        public sealed class Row
        {
            [Tooltip("Id of the diet whose deaths this row counts.")]
            [SerializeField] private string dietId;
            [Tooltip("Entry of the UI string table; {0} is the count.")]
            [SerializeField] private string stringKey;
            [SerializeField] private TMP_Text text;

            public string DietId => dietId;
            public string StringKey => stringKey;

            public void SetText(string value) => text.text = value;
        }
    }
}
