﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PlayerType
{
    human,
    ai
}

[System.Serializable]
public class Player
{
    public PlayerType type = PlayerType.ai;
    public int playerNum;
    public SlotDef handSlotDef;
    public List<CardBartok> hand;

    // Add a card to the hand
    public CardBartok AddCard(CardBartok eCB)
    {
        if (hand == null) hand = new List<CardBartok>();

        // Add the card to the hand
        hand.Add(eCB);

        // Sort the cards by rank using LINQ if this is a human
        if(type == PlayerType.human)
        {
            CardBartok[] cards = hand.ToArray();

            // This is the LINQ call
            cards = cards.OrderBy(cd => cd.rank).ToArray();

            hand = new List<CardBartok>(cards);
        }


        FanHand();
        return eCB;
    }

    // Remove a card from the hand
    public CardBartok RemoveCard(CardBartok cb)
    {
        // If the hand is null or doesn't contain cb, return null
        if (hand == null || !hand.Contains(cb)) return null;
        hand.Remove(cb);
        FanHand();
        return cb;
    }

    public void FanHand()
    {
        // startRot is the rotation about Z of the first card
        float startRot = 0;
        startRot = handSlotDef.rot;
        if(hand.Count > 1)
        {
            startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
        }

        // Move all the cards to their new positions
        Vector3 pos;
        float rot;
        Quaternion rotQ;
        for(int i = 0; i < hand.Count; i++)
        {
            rot = startRot - Bartok.S.handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);

            pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;

            pos = rotQ * pos;

            // Add the base position of the player's hand(which will be at the bottom-center of the fan of the cards)
            pos += handSlotDef.pos;
            pos.z = -0.5f * i;

            // Set the localPosition and rotation of the ith card in the hand
            hand[i].MoveTo(pos, rotQ);
            hand[i].state = CBState.toHand;
            // After the move, CardBartok will set the state to CBState.hand

            /*
            hand[i].transform.localPosition = pos;
            hand[i].transform.rotation = rotQ;
            hand[i].state = CBState.hand;
            */

            hand[i].faceUp = (type == PlayerType.human);

            // Set the SortOrder of the cards so that they overlap properly
            hand[i].SetSortOrder(i * 4);
        }
    }
}
