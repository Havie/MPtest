using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using geniikw.DataRenderer2D;


public class NodeLink : MonoBehaviour
{

    [SerializeField] GameObject _start = default;
    [SerializeField] GameObject _end = default;
    [SerializeField] UILine _line = default;

    [SerializeField] UILine _tex = default;

    private int _controlHandleOffset = 100;

    private void Update()
    {
        if (_start && _end)
        {
            DrawNodeCurve();
        }
    }


    public void DrawNodeCurve()
    {

        var line = _line.line;


        //p1.position = _start.transform.position;
        //p2.position = _end.transform.position;

        //p1.previousControlOffset = Vector3.zero;
        //p2.previousControlOffset = Vector3.zero;

        var pos1 = _start.transform.position;
        var pos2 = _end.transform.position;

        var vecArr = GetNextOffset(pos1, pos2);
        float width = 3;
        line.EditPoint(0, _start.transform.position, vecArr[0], vecArr[1], width);
        line.EditPoint(1, _end.transform.position, vecArr[2], vecArr[3], width);
    }


    private Vector3[] GetNextOffset(Vector3 pos1 , Vector3 pos2)
    {
        Vector3 p1OffsetNext = Vector3.zero;
        Vector3 p1OffsetPrev = Vector3.zero ;

        Vector3 p2OffsetNext = Vector3.zero;
        Vector3 p2OffsetPrev = Vector3.zero;

        int xDir = _controlHandleOffset; /// pos1 - pos2 is too big , its like -289
        //Debug.Log($"what is diff {pos1 - pos2}");
        ///if pos1 LEFT of Pos2
        if (pos1.x > pos2.x)
        {
            xDir = -_controlHandleOffset;
        }

        p1OffsetNext += new Vector3(xDir, 0, 0);
        p2OffsetPrev -= new Vector3(xDir, 0, 0);

        Vector3[] pointArr = new Vector3[4];
        pointArr[0] = p1OffsetNext;
        pointArr[1] = p1OffsetPrev;
        pointArr[2] = p2OffsetNext;
        pointArr[3] = p2OffsetPrev;
        return pointArr;
    }
}
