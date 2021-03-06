using System.Collections.Generic;
using MissionControl.Connection;
using MissionControl.Connection.Commands;
using MissionControl.Data;
using MissionControl.Data.Components;
using MissionControl.UI;
using Moq;
using NUnit.Framework;

namespace MissionControl.Tests
{
    [TestFixture]
    public class UIInterfaceTests
    {
        [Test]
        public void Verify_Stop_Auto_Command_Is_Sent()
        {
            
            TestStandMapping mapping = new TestStandMapping();
            Session session = new Session(mapping);
            DataStore dataStore = new DataStore(session);
            
            Mock<IUserInterface> ui = new Mock<IUserInterface>();
            Mock<LogThread> logThread = new Mock<LogThread>(dataStore);
            IOThread ioThread = new IOThread(dataStore,ref session);
            
            Program p = new Program(dataStore, logThread.Object, ioThread, ui.Object);
            
            p.OnStartAutoSequencePressed();
            p.OnStopAutoSequencePressed();

            List<Command> commands = ioThread.Commands;

            byte[] command1 = commands[0].ToByteData();
            byte[] command2 = commands[1].ToByteData();

            Assert.AreEqual(command1.Length, 2);
            Assert.AreEqual(command2.Length, 2);
            
            Assert.AreEqual(command1[0], 202);
            Assert.AreEqual(command2[0], 202);

            Assert.AreEqual(command1[1], 1);
            Assert.AreEqual(command2[1], 0);
        }
        
        [Test]
        public void Verify_Auto_Params_Sent() {
            TestStandMapping mapping = new TestStandMapping();
            Session session = new Session(mapping);
            DataStore dataStore = new DataStore(session);
            
            Mock<IUserInterface> ui = new Mock<IUserInterface>();
            Mock<LogThread> logThread = new Mock<LogThread>(dataStore);
            IOThread ioThread = new IOThread(dataStore,ref session);
            
            Program p = new Program(dataStore, logThread.Object, ioThread, ui.Object);

            PreferenceManager.Manager.Preferences.Fluid.TodaysPressure = 1.0f;
            
            AutoParameters param = new AutoParameters()
            {
                StartDelay = 20000, // 0x4E20
                IgnitionTime = 4000, // 0x0FA0
                PreStage1Time = 500, // 0x01F4
                PreStage2MaxTime = 3000, // 0x0BB8
                PreStage2StableTime = 250, // 0x00FA
                RampUpStableTime = 250, // 0x00FA
                RampUpMaxTime = 1500, // 0x05DC
                BurnTime = 13000, // 0x32C8
                Shutdown1Time = 500, // 0x01F4
                Shutdown2Time = 500, // 0x01F4
                FlushTime = 2000, // 0x07D0

                PreStage1FuelPosition = 30.0f, // 0x4CCD
                PreStage2FuelPosition = 30.0f, // 0x4CCD
                RampUpFuelPosition = 100.0f, // 0xFFFF
                Shutdown1FuelPosition = 0.0f, // 0x0000
                Shutdown2FuelPosition = 0.0f, // 0x0000
                
                PreStage1OxidPosition = 0.0f, // 0x0000
                PreStage2OxidPosition = 30.0f, // 0x4CCD
                RampUpOxidPosition = 100.0f, // 0xFFFF
                Shutdown1OxidPosition = 0.0f, // 0x0000
                Shutdown2OxidPosition = 0.0f, // 0x0000
        
                PreStage2StablePressure = 4.0f, // barg 462 = 0x01CE
                ChamberPressurePressure = 17.0f, // barg 849 = 0x0351
                EmptyFuelFeedPressureThreshold = 10.0f, // barg 640 = 0x280
                EmptyOxidFeedPressureThreshold = 10.0f // barg 640 = 0x280
            };
            
            p.OnAutoParametersSet(param);
          
            byte[] expected = 
            {
                0xCB,
                
                0x4E, 0x20,
                0x0F, 0xA0,
                0x01, 0xF4,
                0x0B, 0xB8,
                0x00, 0xFA,
                0x00, 0xFA,
                0x05, 0xDC,
                0x32, 0xC8,
                0x01, 0xF4,
                0x01, 0xF4,
                0x07, 0xD0,
                
                0x4C, 0xCD,
                0x4C, 0xCD,
                0xFF, 0xFF,
                0x00, 0x00,
                0x00, 0x00,
                0x00, 0x00,
                0x4C, 0xCD,
                0xFF, 0xFF,
                0x00, 0x00,
                0x00, 0x00,
                
                0x01, 0xCE,
                0x03, 0x51,
                0x02, 0x80,
                0x02, 0x80
            };
            byte[] actual = ioThread.Commands[0].ToByteData();
            Assert.AreEqual(actual, expected);
        }
        
        [Test]
        public void Verify_Tare() {
            TestStandMapping mapping = new TestStandMapping();
            Session session = new Session(mapping);
            DataStore dataStore = new DataStore(session);
            
            Mock<IUserInterface> ui = new Mock<IUserInterface>();
            Mock<LogThread> logThread = new Mock<LogThread>(dataStore);
            IOThread ioThread = new IOThread(dataStore,ref session);
            
            Program p = new Program(dataStore, logThread.Object, ioThread, ui.Object);
            
            LoadComponent l = (mapping.ComponentsByID()[0] as LoadComponent);
            
            Assert.AreEqual(0,l.Newtons());
            l.Set(10);
            Assert.AreEqual(18.83217116,l.Newtons(), 0.01);
            l.Tare();
            l.Set(10);
            Assert.AreEqual(0,l.Newtons(), 0.01);
            l.Set(200);
            Assert.AreEqual(357.8112522,l.Newtons(), 0.01);
        }
    }
}