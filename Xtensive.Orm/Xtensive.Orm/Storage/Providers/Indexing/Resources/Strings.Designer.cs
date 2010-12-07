﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Xtensive.Storage.Providers.Indexing.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Xtensive.Storage.Providers.Indexing.Resources.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not find index &apos;{0}&apos; in storage..
        /// </summary>
        internal static string ExCanNotFindIndexXInStorage {
            get {
                return ResourceManager.GetString("ExCanNotFindIndexXInStorage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The collection is empty.
        /// </summary>
        internal static string ExCollectionIsEmpty {
            get {
                return ResourceManager.GetString("ExCollectionIsEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Current storage provider does not support savepoints (nested transactions)..
        /// </summary>
        internal static string ExCurrentStorageProviderDoesNotSupportSavepoints {
            get {
                return ResourceManager.GetString("ExCurrentStorageProviderDoesNotSupportSavepoints", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Indexing storages support only connection URLs.
        /// </summary>
        internal static string ExIndexingStoragesSupportOnlyConnectionUrls {
            get {
                return ResourceManager.GetString("ExIndexingStoragesSupportOnlyConnectionUrls", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Instance of type &apos;{0}&apos; with specified key is not found..
        /// </summary>
        internal static string ExInstanceXIsNotFound {
            get {
                return ResourceManager.GetString("ExInstanceXIsNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Transaction is already opened..
        /// </summary>
        internal static string ExTransactionIsAlreadyOpened {
            get {
                return ResourceManager.GetString("ExTransactionIsAlreadyOpened", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Transaction is not open..
        /// </summary>
        internal static string ExTransactionIsNotOpened {
            get {
                return ResourceManager.GetString("ExTransactionIsNotOpened", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Index &apos;{0}&apos; with attributes &apos;{1}&apos; is not supported..
        /// </summary>
        internal static string ExUnsupportedIndex {
            get {
                return ResourceManager.GetString("ExUnsupportedIndex", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Session &apos;{0}&apos;. Beginning transaction @ {1}..
        /// </summary>
        internal static string LogSessionXBeginningTransactionWithYIsolationLevel {
            get {
                return ResourceManager.GetString("LogSessionXBeginningTransactionWithYIsolationLevel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Session &apos;{0}&apos;. Commit transaction..
        /// </summary>
        internal static string LogSessionXCommitTransaction {
            get {
                return ResourceManager.GetString("LogSessionXCommitTransaction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Session &apos;{0}&apos;. Make savepoint &apos;{1}&apos;..
        /// </summary>
        internal static string LogSessionXMakeSavepointY {
            get {
                return ResourceManager.GetString("LogSessionXMakeSavepointY", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Session &apos;{0}&apos;. Release savepoint &apos;{1}&apos;..
        /// </summary>
        internal static string LogSessionXReleaseSavepointY {
            get {
                return ResourceManager.GetString("LogSessionXReleaseSavepointY", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Session &apos;{0}&apos;. Rollback to savepoint &apos;{1}&apos;..
        /// </summary>
        internal static string LogSessionXRollbackToSavepointY {
            get {
                return ResourceManager.GetString("LogSessionXRollbackToSavepointY", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Session &apos;{0}&apos;. Rollback transaction..
        /// </summary>
        internal static string LogSessionXRollbackTransaction {
            get {
                return ResourceManager.GetString("LogSessionXRollbackTransaction", resourceCulture);
            }
        }
    }
}