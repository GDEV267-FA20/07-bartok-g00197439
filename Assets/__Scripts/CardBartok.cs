﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CBState includes both states for the game and to... states for movement
public enum CBState
{
    toDrawpile,
    drawpile,
    toHand,
    hand,
    toTarget,
    target,
    discard,
    to,
    idle
}

public class CardBartok : Card
{
    // Static variables are shared by all instances of CardBartok
    static public float moveDuration = 0.5f;
    static public string moveEasing = Easing.InOut;
    static public float cardHeight = 3.5f;
    static public float cardWidth = 2f;

    [Header("Set Dyncamically: CardBartok")]
    public CBState state = CBState.drawpile;

    // Fields to store info the card will use to move and rotate
    public List<Vector3> bezierPts;
    public List<Quaternion> bezierRots;
    public float timeStart, timeDuration;

    // When the card is done moving, it will call reportFinishTo.SendMessage()
    public GameObject reportFinishTo = null;

    // MoveTo tells the card to interpolate to a new position and rotation
    public void MoveTo(Vector3 ePos, Quaternion eRot)
    {
        // Make new interpolation lists for the card
        // Position and Rotation will each have only two points
        bezierPts = new List<Vector3>();
        bezierPts.Add(transform.localPosition);
        bezierPts.Add(ePos);

        bezierRots = new List<Quaternion>();
        bezierRots.Add(transform.rotation);
        bezierRots.Add(eRot);

        if(timeStart == 0)
        {
            timeStart = Time.time;
        }
        // timeSuration always starts the same but can be overwritten later
        timeDuration = moveDuration;

        state = CBState.to;
    }

    public void MoveTo(Vector3 ePos)
    {
        MoveTo(ePos, Quaternion.identity);
    }

    private void Update()
    {
        switch(state)
        {
            case CBState.toHand:
            case CBState.toTarget:
            case CBState.toDrawpile:
            case CBState.to:
                float u = (Time.time - timeStart) / timeDuration;
                float uC = Easing.Ease(u, moveEasing);

                if(u < 0)
                {
                    transform.localPosition = bezierPts[0];
                    transform.rotation = bezierRots[0];
                    return;
                } else if(u >= 1)
                {
                    uC = 1;
                    // Move from the to... state to the proper next state
                    if (state == CBState.toHand) state = CBState.hand;
                    if (state == CBState.toTarget) state = CBState.target;
                    if (state == CBState.toDrawpile) state = CBState.drawpile;
                    if (state == CBState.to) state = CBState.idle;

                    // Move to the final position
                    transform.localPosition = bezierPts[bezierPts.Count - 1];
                    transform.rotation = bezierRots[bezierPts.Count - 1];

                    // Reset timeStart to 0 so it gets overwritten next time
                    timeStart = 0;

                    if(reportFinishTo != null)
                    {
                        reportFinishTo.SendMessage("CBCallBack", this);
                        reportFinishTo = null;
                    } else
                    {
                        // Just let it stay still
                    }
                } else
                {
                    Vector3 pos = Utils.Bezier(uC, bezierPts);
                    transform.localPosition = pos;
                    Quaternion rotQ = Utils.Bezier(uC, bezierRots);
                    transform.rotation = rotQ;
                }
                break;
        }
    }
}
