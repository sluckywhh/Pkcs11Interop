/*
 *  Copyright 2012-2019 The Pkcs11Interop Project
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

/*
 *  Written for the Pkcs11Interop project by:
 *  Jaroslav IMRICH <jimrich@jimrich.sk>
 */

using System;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.LowLevelAPI80;
using NUnit.Framework;

namespace Net.Pkcs11Interop.Tests.LowLevelAPI80
{
    /// <summary>
    /// C_CreateObject, C_DestroyObject, C_CopyObject and C_GetObjectSize tests.
    /// </summary>
    [TestFixture()]
    public class _16_CreateCopyDestroyObjectTest
    {
        /// <summary>
        /// C_CreateObject and C_DestroyObject test.
        /// </summary>
        [Test()]
        public void _01_CreateDestroyObjectTest()
        {
            if (Platform.UnmanagedLongSize != 8 || Platform.StructPackingSize != 0)
                Assert.Inconclusive("Test cannot be executed on this platform");

            CKR rv = CKR.CKR_OK;
            
            using (Pkcs11 pkcs11 = new Pkcs11(Settings.Pkcs11LibraryPath))
            {
                rv = pkcs11.C_Initialize(Settings.InitArgs80);
                if ((rv != CKR.CKR_OK) && (rv != CKR.CKR_CRYPTOKI_ALREADY_INITIALIZED))
                    Assert.Fail(rv.ToString());
                
                // Find first slot with token present
                ulong slotId = Helpers.GetUsableSlot(pkcs11);
                
                ulong session = CK.CK_INVALID_HANDLE;
                rv = pkcs11.C_OpenSession(slotId, (CKF.CKF_SERIAL_SESSION | CKF.CKF_RW_SESSION), IntPtr.Zero, IntPtr.Zero, ref session);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                // Login as normal user
                rv = pkcs11.C_Login(session, CKU.CKU_USER, Settings.NormalUserPinArray, Convert.ToUInt64(Settings.NormalUserPinArray.Length));
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                // Prepare attribute template of new data object
                CK_ATTRIBUTE[] template = new CK_ATTRIBUTE[5];
                template[0] = CkaUtils.CreateAttribute(CKA.CKA_CLASS, CKO.CKO_DATA);
                template[1] = CkaUtils.CreateAttribute(CKA.CKA_TOKEN, true);
                template[2] = CkaUtils.CreateAttribute(CKA.CKA_APPLICATION, Settings.ApplicationName);
                template[3] = CkaUtils.CreateAttribute(CKA.CKA_LABEL, Settings.ApplicationName);
                template[4] = CkaUtils.CreateAttribute(CKA.CKA_VALUE, "Data object content");

                // Create object
                ulong objectId = CK.CK_INVALID_HANDLE;
                rv = pkcs11.C_CreateObject(session, template, Convert.ToUInt64(template.Length), ref objectId);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());

                // In LowLevelAPI we have to free unmanaged memory taken by attributes
                for (int i = 0; i < template.Length; i++)
                {
                    UnmanagedMemory.Free(ref template[i].value);
                    template[i].valueLen = 0;
                }

                // Do something interesting with new object

                // Destroy object
                rv = pkcs11.C_DestroyObject(session, objectId);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());

                rv = pkcs11.C_Logout(session);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                rv = pkcs11.C_CloseSession(session);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                rv = pkcs11.C_Finalize(IntPtr.Zero);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
            }
        }

        /// <summary>
        /// C_CopyObject test.
        /// </summary>
        [Test()]
        public void _02_CopyObjectTest()
        {
            if (Platform.UnmanagedLongSize != 8 || Platform.StructPackingSize != 0)
                Assert.Inconclusive("Test cannot be executed on this platform");

            CKR rv = CKR.CKR_OK;
            
            using (Pkcs11 pkcs11 = new Pkcs11(Settings.Pkcs11LibraryPath))
            {
                rv = pkcs11.C_Initialize(Settings.InitArgs80);
                if ((rv != CKR.CKR_OK) && (rv != CKR.CKR_CRYPTOKI_ALREADY_INITIALIZED))
                    Assert.Fail(rv.ToString());
                
                // Find first slot with token present
                ulong slotId = Helpers.GetUsableSlot(pkcs11);
                
                ulong session = CK.CK_INVALID_HANDLE;
                rv = pkcs11.C_OpenSession(slotId, (CKF.CKF_SERIAL_SESSION | CKF.CKF_RW_SESSION), IntPtr.Zero, IntPtr.Zero, ref session);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                // Login as normal user
                rv = pkcs11.C_Login(session, CKU.CKU_USER, Settings.NormalUserPinArray, Convert.ToUInt64(Settings.NormalUserPinArray.Length));
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                // Create object that can be copied
                ulong objectId = CK.CK_INVALID_HANDLE;
                rv = Helpers.CreateDataObject(pkcs11, session, ref objectId);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                // Copy object
                ulong copiedObjectId = CK.CK_INVALID_HANDLE;
                rv = pkcs11.C_CopyObject(session, objectId, null, 0, ref copiedObjectId);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());

                // Do something interesting with new object

                rv = pkcs11.C_DestroyObject(session, copiedObjectId);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());

                rv = pkcs11.C_DestroyObject(session, objectId);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                rv = pkcs11.C_Logout(session);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                rv = pkcs11.C_CloseSession(session);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                rv = pkcs11.C_Finalize(IntPtr.Zero);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
            }
        }

        /// <summary>
        /// C_GetObjectSize test.
        /// </summary>
        [Test()]
        public void _03_GetObjectSizeTest()
        {
            if (Platform.UnmanagedLongSize != 8 || Platform.StructPackingSize != 0)
                Assert.Inconclusive("Test cannot be executed on this platform");
            
            CKR rv = CKR.CKR_OK;
            
            using (Pkcs11 pkcs11 = new Pkcs11(Settings.Pkcs11LibraryPath))
            {
                rv = pkcs11.C_Initialize(Settings.InitArgs80);
                if ((rv != CKR.CKR_OK) && (rv != CKR.CKR_CRYPTOKI_ALREADY_INITIALIZED))
                    Assert.Fail(rv.ToString());
                
                // Find first slot with token present
                ulong slotId = Helpers.GetUsableSlot(pkcs11);
                
                ulong session = CK.CK_INVALID_HANDLE;
                rv = pkcs11.C_OpenSession(slotId, (CKF.CKF_SERIAL_SESSION | CKF.CKF_RW_SESSION), IntPtr.Zero, IntPtr.Zero, ref session);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                // Login as normal user
                rv = pkcs11.C_Login(session, CKU.CKU_USER, Settings.NormalUserPinArray, Convert.ToUInt64(Settings.NormalUserPinArray.Length));
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                // Create object
                ulong objectId = CK.CK_INVALID_HANDLE;
                rv = Helpers.CreateDataObject(pkcs11, session, ref objectId);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());

                // Determine object size
                ulong objectSize = 0;
                rv = pkcs11.C_GetObjectSize(session, objectId, ref objectSize);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                Assert.IsTrue(objectSize > 0);

                rv = pkcs11.C_DestroyObject(session, objectId);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                rv = pkcs11.C_Logout(session);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                rv = pkcs11.C_CloseSession(session);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
                
                rv = pkcs11.C_Finalize(IntPtr.Zero);
                if (rv != CKR.CKR_OK)
                    Assert.Fail(rv.ToString());
            }
        }
    }
}
