using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pakon.LegacyBridge.Protocol
{

    public static class BridgeOperations
    {
        public const string ProbeTlc = "probe-tlc";
        public const string InitializeTlc = "initialize-tlc";
        public const string GetTlcSessionStatus = "get-tlc-session-status";
        public const string CloseTlcSession = "close-tlc-session";
        public const string InitializeTlxSession = "initialize-tlx-session";
        public const string GetTlxSessionStatus = "get-tlx-session-status";
        public const string ScanRoll = "scan-roll";
        public const string MoveOldestRollToSaveGroup = "move-oldest-roll-to-save-group";
        public const string SaveFramesToDisk = "save-frames-to-disk";
        public const string GetFrames = "get-frames";
        public const string UpdateFrame = "update-frame";
        public const string UpdateFrameFraming = "update-frame-framing";
        public const string ConfigureFrameLayout = "configure-frame-layout";
        public const string InsertFrame = "insert-frame";
        public const string DeleteFrame = "delete-frame";
        public const string RenderFrameToDisk = "render-frame-to-disk";
        public const string RenderFrameToRaw = "render-frame-to-raw";
        public const string CancelScan = "cancel-scan";
        public const string CancelSave = "cancel-save";
        public const string CloseTlxSession = "close-tlx-session";
        public const string BeginTlxTrace = "begin-tlx-trace";
        public const string EndTlxTrace = "end-tlx-trace";
        public const string SnapshotTlxTrace = "snapshot-tlx-trace";
        public const string ScanTlxTraceProfile = "scan-tlx-trace-profile";
        public const string SaveTlxTraceJpegs = "save-tlx-trace-jpegs";
    }

    [DataContract]
    public sealed class BridgeRequest
    {
        public BridgeRequest()
        {
            RequestId = string.Empty;
            Operation = string.Empty;
            Arguments = new Dictionary<string, string>();
        }

        [DataMember(Order = 1, IsRequired = true)]
        public string RequestId { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public string Operation { get; set; }

        /// <summary>Operation-specific scalar inputs. The protocol deliberately avoids exposing COM types.</summary>
        [DataMember(Order = 3)]
        public Dictionary<string, string> Arguments { get; set; }
    }

    [DataContract]
    public sealed class BridgeResponse
    {
        public BridgeResponse()
        {
            RequestId = string.Empty;
            Values = new Dictionary<string, string>();
        }

        [DataMember(Order = 1, IsRequired = true)]
        public string RequestId { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public bool Succeeded { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Error { get; set; }

        [DataMember(Order = 4)]
        public Dictionary<string, string> Values { get; set; }
    }
}
