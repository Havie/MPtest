#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

    /// <summary>
    /// A Wrapper class so buttons can find the current GameManager throughout scene changes
    /// </summary>
	public class GameManagerWrapper : MonoBehaviour
	{
   public void CallRoundDurationChanged(IntWrapper val)
    {
        GameManager.Instance.RoundDurationChanged(val);
    }
    public void CallRoundDurationChanged(int duration) 
    {
        GameManager.Instance.RoundDurationChanged(duration);
    }
    public void CallOrderFreqChanged(IntWrapper val)
    {
        GameManager.Instance.OrderFreqChanged(val);
    }
    public void CallBatchChanged(IntWrapper val) 
    {
        GameManager.Instance.BatchChanged(val);
    }
    public void CallOnDeliveryTimeChanged(IntWrapper val)
    {
        GameManager.Instance.DeliveryTimeChanged(val);
    }

    public void CallBatchChanged(int val)
    {
        GameManager.Instance.BatchChanged(val);
    }
    public void CallAutoSendChanged(bool cond)
    {
        GameManager.Instance.AutoSendChanged(cond);
    }
    public void CallAddChaoticChanged(bool cond)
    {
        GameManager.Instance.AddChaoticChanged(cond);
    }
    public void CallIsStackableChanged(bool cond)
    {
        GameManager.Instance.IsStackableChanged(cond);
    }
    public void CallWorkStationArrangementChanged(bool cond)
    {
        GameManager.Instance.WorkStationArrangementChanged(cond);
    }

    public void CallWorkStationTaskChanged(bool cond)
    {
        GameManager.Instance.WorkStationTaskChanged(cond);
    }
    public void CallDecreasedChangedOverTimeChanged(bool cond)
    {
        GameManager.Instance.DecreasedChangedOverTimeChanged(cond);
    }
    public void CallHUDManagementChanged(bool cond)
    {
        GameManager.Instance.HUDManagementChanged(cond);
    }
    public void CallHostDefectPausingChanged(bool cond)
    {
        GameManager.Instance.HostDefectPausingChanged(cond);
    }
    public void CallOnStartWithWIP(bool cond)
    {
        GameManager.Instance.StartWithWIPChanged(cond);
    }
}
