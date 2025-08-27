using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NativeWebSocket;

using System.Net;
using System.Net.Sockets;

[System.Serializable]
public class Tongtai_tmv720_simulate
{
    public string StepIndex;
    public string FilePath;
    public string Line;
    public string FileNo;
    public string LineNo;
    public string WorkTime;
    public string StepDuration_s;
    public string CyclePeriod_s;
    public string ToolId;
    public string Feedrate_mmdmin;
    public string SpindleSpeed_rpm;
    public string SpindleDirection;
    public string IsTouched;
    public string RadialWidth;
    public string AxialDepth;
    public string Mrr_mm3ds;
    public string ChipThickness;
    public string CL_X;
    public string CL_Y;
    public string CL_Z;
    public string CL_A;
    public string CL_B;
    public string CL_C;
    public string MC_X;
    public string MC_Y;
    public string MC_Z;
    public string MC_B_deg;
    public string MC_C_deg;
    public string FeedPerTooth_mm;
    public string CuttingVelocity_mdmin;
    public string AbsAvgForce;
    public string MaxAbsForce;
    public string MaxAbsForceSlope_NdDeg;
    public string AvgAxialTorque_Nm;
    public string MaxAxialTorqueToToolByToolTip_Nm;
    public string FrictionPower_W;
    public string PowerWithoutFriction_W;
    public string AxialPowerTakenByWorkpiece_W;
    public string AbsAxialPower_W;
    public string MinAbsMomentByObservationPoint_Nm;
    public string MaxAbsMomentByObservationPoint_Nm;
    public string ChipVolume_mm3;
    public string AvgForceToToolOnToolRunningCoordinate_X;
    public string AvgForceToToolOnToolRunningCoordinate_Y;
    public string AvgForceToToolOnToolRunningCoordinate_Z;
    public string AvgForceToWorkpieceOnWorkpieceCoordinate_X;
    public string AvgForceToWorkpieceOnWorkpieceCoordinate_Y;
    public string AvgForceToWorkpieceOnWorkpieceCoordinate_Z;
    public string MaxMomentToToolOnToolRunningCoordinateByObservationPoint_Nm_X;
    public string MaxMomentToToolOnToolRunningCoordinateByObservationPoint_Nm_Y;
    public string MaxMomentToToolOnToolRunningCoordinateByObservationPoint_Nm_Z;
    public string AccumulatedToolWear_mm;
    public string Vibration_X;
    public string Vibration_Y;
    public string Vibration_Z;
    public string TLMNo;
    public string TLMUpdate;
    public string TLMRUL;
    public string IPMUpdate;
    public string IPMDHI;
    public string IPMRUL;

}

[System.Serializable]
public class WebSocketCustomData
{
    public string WebSocketServerSendingTime;
    public bool WSLatencyTestFlag;
    public int WSLatencyTestID;
}

[System.Serializable]
public class WebSocketData
{
    public Tongtai_tmv720_simulate JSON;
    public WebSocketCustomData WebSocketInfo;
    public string DateTime;

}

public class WebSocketConnectTest : MonoBehaviour
{
    const string websocket_address = "address";
    string ws_msg = "";
    bool new_ws_msg_flag = false;
    WebSocket websocket;
    Tongtai_tmv720_simulate CurrentTmvMsg;
    WebSocketData webSocketData;
    public DashboardUpdate DashboardUpdate;
    public PlatformMoveX PlatformMoveX;
    public PlatformMoveY PlatformMoveY;
    public PlatformMoveZ PlatformMoveZ;

    async void Start()
    {
        WSStart();
    }

    async void WSStart()
    {
        websocket = new WebSocket(websocket_address);

        websocket.OnOpen += () =>
        {
            Debug.Log("TT - Connection open !");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("TT - Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("TT - Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            ws_msg = System.Text.Encoding.UTF8.GetString(bytes);
            new_ws_msg_flag = true;
        };
        await websocket.Connect();
    }
    void Update()
    {
        if (websocket.State == WebSocketState.Closed)
        {
            WSStart();
        }
        if (new_ws_msg_flag)
        {
            webSocketData = JsonUtility.FromJson<WebSocketData>(ws_msg);
            string wsdata = JsonUtility.ToJson(webSocketData);
            string temp_MachineData = JsonUtility.ToJson(webSocketData.JSON);
            Tongtai_tmv720_simulate temp_tmvData = JsonUtility.FromJson<Tongtai_tmv720_simulate>(temp_MachineData);
            CurrentTmvMsg = temp_tmvData;
            new_ws_msg_flag = false;

            PlatformMoveX.setCoordinates(CurrentTmvMsg.MC_X);
            PlatformMoveY.setCoordinates(CurrentTmvMsg.MC_Y);
            PlatformMoveZ.setCoordinates(CurrentTmvMsg.MC_Z);

            DashboardUpdate.takeValue(temp_MachineData);
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }
}
