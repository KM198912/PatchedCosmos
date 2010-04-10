﻿//TODO: Move both of these to project options...
// In fact also eliminate TCP server and keep only Pipes
#define DEBUG_CONNECTOR_TCP_SERVER
//#define DEBUG_CONNECTOR_PIPE_SERVER
//
#define VM_QEMU
//#define VM_VMWare

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio;
using System.Diagnostics;
using Cosmos.Debug.Common.CDebugger;
using System.Collections.ObjectModel;
using System.IO;
using Cosmos.Compiler.Debug;

namespace Cosmos.Debug.VSDebugEngine
{
    public class AD7Process : IDebugProcess2
    {
        internal string mISO;
        internal Guid mID = Guid.NewGuid();
        private Process mProcess;
        private ProcessStartInfo mProcessStartInfo;
        private EngineCallback mCallback;
        private AD7Thread mThread;
        private AD7Engine mEngine;
        private DebugEngine mDebugEngine;
        internal ReverseSourceInfos mReverseSourceMappings;
        internal SourceInfos mSourceMappings;
        internal uint? mCurrentAddress = null;

        internal AD7Process(string aISOFile, EngineCallback aCallback, AD7Engine aEngine, IDebugPort2 aPort)
        {
            mISO = aISOFile;

            mProcessStartInfo = new ProcessStartInfo(Path.Combine(PathUtilities.GetVSIPDir(), "Cosmos.Debug.HostProcess.exe"));

#if DEBUG_CONNECTOR_TCP_SERVER
            var xDebugConnectorStr = "-serial tcp:127.0.0.1:4444";
#endif
#if DEBUG_CONNECTOR_PIPE_SERVER
            // The pipe name is \\.\pipe\com_1
#endif

#if VM_QEMU
            // Start QEMU
            mProcessStartInfo.Arguments = String.Format("\"{0}\" -L \"{1}\" -cdrom \"{2}\" -boot d {3}", Path.Combine(PathUtilities.GetQEmuDir(), "qemu.exe").Replace('\\', '/'), PathUtilities.GetQEmuDir(), mISO.Replace("\\", "/"), xDebugConnectorStr);
            mProcessStartInfo.UseShellExecute = false;
            mProcessStartInfo.RedirectStandardInput = true;
            mProcessStartInfo.RedirectStandardError = true;
            mProcessStartInfo.RedirectStandardOutput = true;
            mProcessStartInfo.CreateNoWindow = true;
#endif
#if VM_VMWare
            mProcessStartInfo.Arguments = @"C:\source\Cosmos\Build\VMWare\Workstation\Cosmos.vmx";
            mProcessStartInfo.UseShellExecute = true;
            mProcessStartInfo.RedirectStandardInput = false;
            mProcessStartInfo.RedirectStandardError = false;
            mProcessStartInfo.RedirectStandardOutput = false;
            mProcessStartInfo.CreateNoWindow = false;
#endif

            var xLabelByAddressMapping = Cosmos.Debug.Common.CDebugger.SourceInfo.ReadFromFile(Path.ChangeExtension(aISOFile, "cmap"));
            if (xLabelByAddressMapping.Count == 0)
            {
                throw new Exception("Debug data not found: LabelByAddressMapping");
            }
            mSourceMappings = Cosmos.Debug.Common.CDebugger.SourceInfo.GetSourceInfo(xLabelByAddressMapping, Path.ChangeExtension(aISOFile, ".cxdb"));
            if (mSourceMappings.Count == 0)
            {
                throw new Exception("Debug data not found: SourceMappings");
            }
            mReverseSourceMappings = new ReverseSourceInfos(mSourceMappings);
            mDebugEngine = new DebugEngine();
#if DEBUG_CONNECTOR_TCP_SERVER
            mDebugEngine.DebugConnector = new Cosmos.Debug.Common.CDebugger.DebugConnectorTCPServer();
#endif
            mDebugEngine.TraceReceived += new Action<Cosmos.Compiler.Debug.MsgType, uint>(mDebugEngine_TraceReceived);
            mDebugEngine.TextReceived += new Action<string>(mDebugEngine_TextReceived);
#if DEBUG_CONNECTOR_TCP_SERVER
            mDebugEngine.DebugConnector.ConnectionLost = new Action<Exception>(delegate { mEngine.Callback.OnProcessExit(0); });
#endif

            System.Threading.Thread.Sleep(250);
            mProcess = Process.Start(mProcessStartInfo);
            mProcess.EnableRaisingEvents = true;
            mProcess.Exited += new EventHandler(mProcess_Exited);
            System.Threading.Thread.Sleep(250);
//TODO: Make redirection non VM specific, and a boolean. Others besides QEMU might use it although VMWare does not.
#if VM_QEMU
            if (mProcess.HasExited)
            {
                Trace.WriteLine("Error while running: " + mProcess.StandardError.ReadToEnd());
                Trace.WriteLine(mProcess.StandardOutput.ReadToEnd());
                Trace.WriteLine("ExitCode: " + mProcess.ExitCode);
                throw new Exception("Error while starting application");
            }
#endif
            mCallback = aCallback;
            mEngine = aEngine;
            mThread = new AD7Thread(aEngine, this);
            mCallback.OnThreadStart(mThread);
            mPort = aPort;
        }

        public void SetBreakpointAddress(uint aAddress)
        {
            mDebugEngine.DebugConnector.SetBreakpointAddress(aAddress);
        }

        void mDebugEngine_TextReceived(string obj)
        {
            mCallback.OnOutputString(obj + "\r\n");
        }

        internal AD7Thread Thread
        {
            get
            {
                return mThread;
            }
        }

        void mDebugEngine_TraceReceived(Cosmos.Compiler.Debug.MsgType arg1, uint arg2)
        {
            switch (arg1)
            {
                case Cosmos.Compiler.Debug.MsgType.BreakPoint:
                    {
                        //((IDebugBreakEvent2)null).

                        //var xSourceInfo = mSourceMappings[arg2];

                        //mCallback.OnOutputString("Try to break now");
                        var xActualAddress = arg2 - 5; // - 5 to correct the addres:
                        // when doing a CALL, the return address is pushed, but that's the address of the next instruction, after CALL. call is 5 bytes (for now?)
                        mEngine.Callback.OnOutputString("Hit Breakpoint 0x" + xActualAddress.ToString("X8").ToUpper());
                        var xActionPoints = new List<object>();
                        var xBoundBreakpoints = new List<IDebugBoundBreakpoint2>();
                        foreach (var xBP in mEngine.m_breakpointManager.m_pendingBreakpoints)
                        {
                            foreach(var xBBP in xBP.m_boundBreakpoints){
                                if (xBBP.m_address == xActualAddress)
                                {
                                    xBoundBreakpoints.Add(xBBP);
                                }
                            }
                        }

                        mCurrentAddress = xActualAddress;
                        //mCallback.onb
                        mCallback.OnBreakpoint(mThread, new ReadOnlyCollection<IDebugBoundBreakpoint2>(xBoundBreakpoints), xActualAddress);
                        //mEngine.Callback.OnBreakComplete(mThread, );
                        mEngine.AfterBreak = true;
                        //mEngine.Callback.OnBreak(mThread);
                        break;
                    }
                default:
                    Console.WriteLine("TraceReceived: {0}", arg1);
                    break;
            }
        }


        #region IDebugProcess2 Members

        public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
        {
            Trace.WriteLine(new StackTrace(false).GetFrame(0).GetMethod().GetFullName());
            throw new NotImplementedException();
        }

        public int CanDetach()
        {
            throw new NotImplementedException();
        }

        public int CauseBreak()
        {
            throw new NotImplementedException();
        }

        public int Detach()
        {
            throw new NotImplementedException();
        }

        public int EnumPrograms(out IEnumDebugPrograms2 ppEnum)
        {
            throw new NotImplementedException();
        }


        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            var xEnum = new AD7ThreadEnum(new IDebugThread2[] { mThread });
            ppEnum = xEnum;
            return VSConstants.S_OK;
        }

        public int GetAttachedSessionName(out string pbstrSessionName)
        {
            throw new NotImplementedException();
        }

        public int GetInfo(uint Fields, PROCESS_INFO[] pProcessInfo)
        {                  throw new NotImplementedException();
        }

        public int GetName(uint gnType, out string pbstrName)
        {
            throw new NotImplementedException();
        }

        public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId)
        {
            Trace.WriteLine(new StackTrace(false).GetFrame(0).GetMethod().GetFullName());
            pProcessId[0].dwProcessId = (uint)mProcess.Id;
            pProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
            return VSConstants.S_OK;
        }

        private IDebugPort2 mPort = null;

        public int GetPort(out IDebugPort2 ppPort)
        {
            if (mPort == null)
            {
                throw new Exception("Error");
            }
            ppPort = mPort;
            return VSConstants.S_OK;
        }

        public int GetProcessId(out Guid pguidProcessId)
        {
            Trace.WriteLine(new StackTrace(false).GetFrame(0).GetMethod().GetFullName());
            pguidProcessId = mID;
            return VSConstants.S_OK;
        }

        public int GetServer(out IDebugCoreServer2 ppServer)
        {
            throw new NotImplementedException();
        }

        public int Terminate()
        {
            mProcess.Kill();
            return VSConstants.S_OK;
        }

        #endregion

        internal void ResumeFromLaunch()
        {
#if DEBUG_CONNECTOR_TCP_SERVER
            //mProcess.StandardInput.WriteLine("");
#endif
        }

        void mProcess_Exited(object sender, EventArgs e)
        {
#if VM_QEMU
            Trace.WriteLine("Error while running: " + mProcess.StandardError.ReadToEnd());
            Trace.WriteLine(mProcess.StandardOutput.ReadToEnd());
#endif
            //AD7ThreadDestroyEvent.Send(mEngine, mThread, (uint)mProcess.ExitCode);
            //mCallback.OnProgramDestroy((uint)mProcess.ExitCode);
            //mCallback.OnProcessExit((uint)mProcess.ExitCode);
        }

        internal void Continue()
        {
            mCurrentAddress = null;
            mDebugEngine.DebugConnector.SendCommand((byte)Command.Break);
        }

        internal void Step()
        {
            mDebugEngine.DebugConnector.SendCommand((byte)Command.Step);
        }
    }
}