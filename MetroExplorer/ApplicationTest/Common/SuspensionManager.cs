using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ApplicationTest.Common
{
    /// <summary>
    /// SuspensionManager capture l'état de session global pour simplifier la gestion de la durée de vie des processus
    /// pour une application. Notez que l'état de session sera automatiquement supprimé sous diverses
    /// conditions ; il doit uniquement être utilisé pour stocker des informations qu'il est utile
    /// de conserver d'une session à une autre, mais qui doivent être supprimées lorsque l'application se bloque ou est
    /// mise à jour.
    /// </summary>
    internal sealed class SuspensionManager
    {
        private static Dictionary<string, object> _sessionState = new Dictionary<string, object>();
        private static List<Type> _knownTypes = new List<Type>();
        private const string sessionStateFilename = "_sessionState.xml";

        /// <summary>
        /// Permet d'accéder à l'état de session global pour la session active. Cet état est
        /// sérialisé par <see cref="SaveAsync"/> et restauré par
        /// <see cref="RestoreAsync"/>. Par conséquent, les valeurs doivent être sérialisables par
        /// <see cref="DataContractSerializer"/> et aussi compactes que possible. Des chaînes
        /// et autres types de données autonomes sont fortement recommandés.
        /// </summary>
        public static Dictionary<string, object> SessionState
        {
            get { return _sessionState; }
        }

        /// <summary>
        /// Liste de types personnalisés fournis au <see cref="DataContractSerializer"/> lors de
        /// la lecture et de l'écriture de l'état de session. Initialement, d'autres types vides peuvent être
        /// ajoutés pour personnaliser le processus de sérialisation.
        /// </summary>
        public static List<Type> KnownTypes
        {
            get { return _knownTypes; }
        }

        /// <summary>
        /// Enregistre le <see cref="SessionState"/> actuel. Toutes les instances de <see cref="Frame"/>
        /// inscrites avec <see cref="RegisterFrame"/> conservent également leur
        /// pile de navigation actuelle, ce qui permet à leur <see cref="Page"/> active
        /// d'enregistrer son état.
        /// </summary>
        /// <returns>Tâche asynchrone qui reflète quand l'état de session a été enregistré.</returns>
        public static async Task SaveAsync()
        {
            try
            {
                // Enregistre l'état de navigation pour tous les frames inscrits
                foreach (var weakFrameReference in _registeredFrames)
                {
                    Frame frame;
                    if (weakFrameReference.TryGetTarget(out frame))
                    {
                        SaveFrameNavigationState(frame);
                    }
                }

                // Sérialise l'état de session de manière synchrone pour éviter un accès asynchrone à un
                // état
                MemoryStream sessionData = new MemoryStream();
                DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, object>), _knownTypes);
                serializer.WriteObject(sessionData, _sessionState);

                // Obtient un flux de sortie pour le fichier SessionState file et écrit l'état de manière asynchrone
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(sessionStateFilename, CreationCollisionOption.ReplaceExisting);
                using (Stream fileStream = await file.OpenStreamForWriteAsync())
                {
                    sessionData.Seek(0, SeekOrigin.Begin);
                    await sessionData.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }
            }
            catch (Exception e)
            {
                throw new SuspensionManagerException(e);
            }
        }

        /// <summary>
        /// Restaure le <see cref="SessionState"/> précédemment enregistré. Toutes les instances de <see cref="Frame"/>
        /// inscrites avec <see cref="RegisterFrame"/> restaurent également leur état de navigation
        /// précédent, ce qui permet à leur <see cref="Page"/> active de restaurer son
        /// d'une application.
        /// </summary>
        /// <returns>Tâche asynchrone qui reflète quand un état de session a été lu. Le
        /// contenu de <see cref="SessionState"/> ne doit pas être pris en compte tant que cette tâche n'est pas
        /// achevée.</returns>
        public static async Task RestoreAsync()
        {
            _sessionState = new Dictionary<String, Object>();

            try
            {
                // Obtient le flux d'entrée pour le fichier SessionState
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(sessionStateFilename);
                using (IInputStream inStream = await file.OpenSequentialReadAsync())
                {
                    // Désérialise l'état de session
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, object>), _knownTypes);
                    _sessionState = (Dictionary<string, object>)serializer.ReadObject(inStream.AsStreamForRead());
                }

                // Restaure l'état enregistré des frames inscrits
                foreach (var weakFrameReference in _registeredFrames)
                {
                    Frame frame;
                    if (weakFrameReference.TryGetTarget(out frame))
                    {
                        frame.ClearValue(FrameSessionStateProperty);
                        RestoreFrameNavigationState(frame);
                    }
                }
            }
            catch (Exception e)
            {
                throw new SuspensionManagerException(e);
            }
        }

        private static DependencyProperty FrameSessionStateKeyProperty =
            DependencyProperty.RegisterAttached("_FrameSessionStateKey", typeof(String), typeof(SuspensionManager), null);
        private static DependencyProperty FrameSessionStateProperty =
            DependencyProperty.RegisterAttached("_FrameSessionState", typeof(Dictionary<String, Object>), typeof(SuspensionManager), null);
        private static List<WeakReference<Frame>> _registeredFrames = new List<WeakReference<Frame>>();

        /// <summary>
        /// Inscrit une instance de <see cref="Frame"/> pour que son historique de navigation puisse être enregistré dans
        /// et restauré à partir de <see cref="SessionState"/>. Les frames doivent être inscrits une seule fois,
        /// immédiatement après la création s'ils doivent participer à la gestion de l'état de session. Lors de
        /// l'inscription, si l'état a déjà été restauré pour la clé spécifiée,
        /// l'historique de navigation est immédiatement restauré. Les appels ultérieurs de
        /// <see cref="RestoreAsync"/> restaurent également l'historique de navigation.
        /// </summary>
        /// <param name="frame">Instance dont l'historique de navigation doit être géré par
        /// <see cref="SuspensionManager"/></param>
        /// <param name="sessionStateKey">Clé unique dans <see cref="SessionState"/> utilisée pour
        /// enregistrer des informations relatives à la navigation.</param>
        public static void RegisterFrame(Frame frame, String sessionStateKey)
        {
            if (frame.GetValue(FrameSessionStateKeyProperty) != null)
            {
                throw new InvalidOperationException("Frames can only be registered to one session state key");
            }

            if (frame.GetValue(FrameSessionStateProperty) != null)
            {
                throw new InvalidOperationException("Frames must be either be registered before accessing frame session state, or not registered at all");
            }

            // Utilise une propriété de dépendance pour associer la clé de session à un frame et conserver une liste des frames dont
            // l'état de navigation doit être géré
            frame.SetValue(FrameSessionStateKeyProperty, sessionStateKey);
            _registeredFrames.Add(new WeakReference<Frame>(frame));

            // Vérifie si l'état de navigation peut être restauré
            RestoreFrameNavigationState(frame);
        }

        /// <summary>
        /// Dissocie un <see cref="Frame"/> précédemment inscrit par <see cref="RegisterFrame"/>
        /// de <see cref="SessionState"/>. Tout état de navigation précédemment capturé sera
        /// supprimé.
        /// </summary>
        /// <param name="frame">Instance dont l'historique de navigation ne doit plus être
        /// géré.</param>
        public static void UnregisterFrame(Frame frame)
        {
            // Supprime l'état de session et supprime le frame de la liste des frames dont l'état de navigation
            // sera enregistré (avec les références faibles qui ne sont plus accessibles)
            SessionState.Remove((String)frame.GetValue(FrameSessionStateKeyProperty));
            _registeredFrames.RemoveAll((weakFrameReference) =>
            {
                Frame testFrame;
                return !weakFrameReference.TryGetTarget(out testFrame) || testFrame == frame;
            });
        }

        /// <summary>
        /// Permet de stocker l'état de session associé au <see cref="Frame"/> spécifié.
        /// L'état de session des frames inscrits précédemment à l'aide de <see cref="RegisterFrame"/> est
        /// enregistré et restauré automatiquement dans le cadre du
        /// <see cref="SessionState"/> global. Les frames qui ne sont pas inscrits ont un état transitoire
        /// qui peut néanmoins être utile lors de la restauration de pages qui ont été supprimées du
        /// cache de navigation.
        /// </summary>
        /// <remarks>Les applications peuvent utiliser <see cref="LayoutAwarePage"/> pour gérer
        /// l'état spécifique aux pages, au lieu de gérer directement l'état de session de frames.</remarks>
        /// <param name="frame">Instance pour laquelle l'état de session est requis.</param>
        /// <returns>Collection d'états pour lequel est utilisé le même mécanisme de sérialisation que
        /// <see cref="SessionState"/>.</returns>
        public static Dictionary<String, Object> SessionStateForFrame(Frame frame)
        {
            var frameState = (Dictionary<String, Object>)frame.GetValue(FrameSessionStateProperty);

            if (frameState == null)
            {
                var frameSessionKey = (String)frame.GetValue(FrameSessionStateKeyProperty);
                if (frameSessionKey != null)
                {
                    // Les frames inscrits reflètent l'état de session correspondant
                    if (!_sessionState.ContainsKey(frameSessionKey))
                    {
                        _sessionState[frameSessionKey] = new Dictionary<String, Object>();
                    }
                    frameState = (Dictionary<String, Object>)_sessionState[frameSessionKey];
                }
                else
                {
                    // Les frames non inscrits ont un état transitoire
                    frameState = new Dictionary<String, Object>();
                }
                frame.SetValue(FrameSessionStateProperty, frameState);
            }
            return frameState;
        }

        private static void RestoreFrameNavigationState(Frame frame)
        {
            var frameState = SessionStateForFrame(frame);
            if (frameState.ContainsKey("Navigation"))
            {
                frame.SetNavigationState((String)frameState["Navigation"]);
            }
        }

        private static void SaveFrameNavigationState(Frame frame)
        {
            var frameState = SessionStateForFrame(frame);
            frameState["Navigation"] = frame.GetNavigationState();
        }
    }
    public class SuspensionManagerException : Exception
    {
        public SuspensionManagerException()
        {
        }

        public SuspensionManagerException(Exception e)
            : base("SuspensionManager failed", e)
        {

        }
    }
}
