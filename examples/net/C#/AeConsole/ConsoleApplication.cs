#region Copyright (c) 2011-2019 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2019 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
// 
// Purpose: 
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// SPDX-License-Identifier: MIT
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2019 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using OpcClientSdk;
using OpcClientSdk.Ae;
#endregion

namespace AeConsole
{

    /// <summary>
    /// Simple OPC AE Client Application
    /// </summary>
    class ConsoleApplication
    {

        #region Event Handlers
        /// <summary>
        /// Receives new events from the AE Server.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="refresh"></param>
        /// <param name="lastRefresh"></param>
        public void OnDataChangedEvent(TsCAeEventNotification[] events, bool refresh, bool lastRefresh)
        {
            foreach (TsCAeEventNotification AeEvent in events)
            {
                Console.WriteLine("New Event:");
                Console.WriteLine("----------");
                Console.Write("\tSource:\t\t"); Console.WriteLine(AeEvent.SourceID);
                Console.Write("\tTime Stamp:\t"); Console.WriteLine(AeEvent.Time.ToString());
                Console.Write("\tMessage:\t"); Console.WriteLine(AeEvent.Message);
                Console.Write("\tType:\t\t"); Console.WriteLine(AeEvent.EventType);
                Console.Write("\tCategory:\t"); Console.WriteLine(AeEvent.EventCategory);
                Console.Write("\tSeverity:\t"); Console.WriteLine(AeEvent.Severity);
                // Values which are present only for Condition-Related Events
                if (AeEvent.EventType == TsCAeEventType.Condition)
                {
                    Console.Write("\tCondition:\t"); Console.WriteLine(AeEvent.ConditionName);
                    Console.Write("\tSubcondition:\t"); Console.WriteLine(AeEvent.SubConditionName);
                    Console.Write("\tChange Mask:\t"); Console.WriteLine(AeEvent.ChangeMask);
                    Console.Write("\tNew State:\t"); Console.WriteLine(AeEvent.NewState);
                    // The following attribute is only supported if full DA/AE/HDA version is used
                    // Console.Write("\tQuality:\t"); Console.WriteLine(AeEvent.Quality);
                    Console.Write("\tAck Required:\t"); Console.WriteLine(AeEvent.AckRequired ? "True" : "False");
                    Console.Write("\tActive Time:\t"); Console.WriteLine(AeEvent.ActiveTime.ToString());
                    Console.Write("\tCookie:\t\t"); Console.WriteLine(AeEvent.Cookie); ;

                    if ((AeEvent.NewState & (int)TsCAeConditionState.Acknowledged) == 1)
                    {
                        Console.Write("\tActorID:\t"); Console.WriteLine(AeEvent.ActorID); ;
                    }
                } // Condition-related Events

                else if (AeEvent.EventType == TsCAeEventType.Tracking)
                {
                    Console.Write("\tActorID:\t"); Console.WriteLine(AeEvent.ActorID);
                }
                if (AeEvent.Attributes.Count > 0)
                {
                    for (int i = 0; i < AeEvent.Attributes.Count; i++)
                    {
                        Console.Write("\tAttribute ");
                        Console.Write(i);
                        Console.Write(":\t"); Console.WriteLine(AeEvent.Attributes[i]);
                    }
                }
                Console.WriteLine();
            }
        }

        #endregion

        #region OPC Sample Functionality

        public void DoOPCCalls()
        {
            try
            {
                const string serverUrl = "opcae://localhost/Technosoftware.AeSample";

                Console.WriteLine();
                Console.WriteLine("Simple OPC AE Client based on the OPC AE Client SDK .NET");
                Console.WriteLine("--------------------------------------------------------");
                Console.Write("   Press <Enter> to connect to "); Console.WriteLine(serverUrl);
                Console.ReadLine();
                Console.WriteLine("   Please wait...");

                //LicenseHandler.Validate("licenseOwner", "licenseKey");

                TsCAeServer myAeServer = new TsCAeServer();

                // Connect to the server
                myAeServer.Connect(serverUrl);

                Console.WriteLine("   Connected, press <Enter> to create an active subscription and press <Enter>");
                Console.WriteLine("   again to deactivate the subscription. This stops the reception of new events.");
                Console.ReadLine();

                TsCAeSubscriptionState state = new TsCAeSubscriptionState { Active = true, BufferTime = 0, MaxSize = 0, ClientHandle = 100, Name = "Simple Event Subscription" };

                TsCAeCategory[] categories = myAeServer.QueryEventCategories((int)TsCAeEventType.Condition);
                TsCAeAttribute[] attributes = null;
                attributes = myAeServer.QueryEventAttributes(categories[0].ID);

                int[] attributeIDs = new int[2];

                attributeIDs[0] = attributes[0].ID;
                attributeIDs[1] = attributes[1].ID;

                TsCAeSubscription subscription;
                subscription = (TsCAeSubscription)myAeServer.CreateSubscription(state);
                subscription.DataChangedEvent += OnDataChangedEvent;
                subscription.SelectReturnedAttributes(categories[0].ID, attributeIDs);
                Console.ReadLine();

                subscription.DataChangedEvent -= OnDataChangedEvent;
                Console.WriteLine("   Subscription deactivated, press <Enter> to remove the subscription and disconnect from the server.");

                subscription.Dispose();									// Remove subscription
                myAeServer.Disconnect();								// Disconnect from server
                myAeServer.Dispose();									// Dispose server object

                Console.ReadLine();
                Console.WriteLine("   Disconnected from the server.");
                Console.WriteLine();
            }
            catch (OpcResultException e)
            {
                Console.WriteLine(String.Format("   {0}", e.Message));
                Console.ReadLine();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("   {0}", e.Message));
                Console.ReadLine();
                return;
            }

        }

        #endregion

        #region Main Entry

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConsoleApplication myClass = new ConsoleApplication();

            myClass.DoOPCCalls();
        }

        #endregion
    }
}
