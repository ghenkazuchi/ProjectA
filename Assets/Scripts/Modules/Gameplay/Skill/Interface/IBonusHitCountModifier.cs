using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBonusHitCountModifier
{
    int GetBonusHitCount();
    IEnumerator Consume();
}
