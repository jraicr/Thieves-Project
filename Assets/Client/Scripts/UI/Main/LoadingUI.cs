using System.Collections.Generic;
using Barebones.Utils;
using UnityEngine;
using UnityEngine.UI;
using Barebones.MasterServer;

namespace Thieves.Client.UI {
		/// <summary>
		///     Represents a loading window
		/// </summary>
		public class LoadingUI : MonoBehaviour {
				private EventsChannel events;

				private GenericPool<LoadingUIItem> pool;

				private Dictionary<int, LoadingUIItem> visibleLoadingItems;
				public string defaultLoadingMessage = "Loading...";

				public LayoutGroup itemsGroup;
				public LoadingUIItem loadingItemPrefab;

				public Image rotatingImage;

				protected bool hasSubscribedToEvents;

				private void Awake() {
						SubscribeToEvents();
				}

				public void SubscribeToEvents() {
						if (hasSubscribedToEvents)
								return;

						hasSubscribedToEvents = true;

						Msf.Events.Subscribe(Msf.EventNames.ShowLoading, OnLoadingEvent);
				}

				private void Update() {
						rotatingImage.transform.Rotate(Vector3.forward, Time.deltaTime * 360 * 2);
				}

				private void OnEnable() {
						gameObject.transform.SetAsLastSibling();
				}

				private void OnLoadingEvent(object arg1, object arg2) {
						HandleEvent(arg1 as EventsChannel.Promise, arg2 as string);
				}

				protected virtual void HandleEvent(EventsChannel.Promise promise, string message) {
						if (visibleLoadingItems == null)
								visibleLoadingItems = new Dictionary<int, LoadingUIItem>();

						if (pool == null)
								pool = new GenericPool<LoadingUIItem>(loadingItemPrefab);

						// If this is the first item to get to the list
						if (visibleLoadingItems.Count == 0)
								gameObject.SetActive(true);

						OnLoadingStarted(promise, message ?? defaultLoadingMessage);
						promise.Subscribe(OnLoadingFinished);
				}

				protected virtual void OnLoadingStarted(EventsChannel.Promise promise, string message) {
						// Create an item
						var newItem = pool.GetResource();
						newItem.id = promise.EventId;
						newItem.message.text = message;

						// Move item to the list
						newItem.transform.SetParent(itemsGroup.transform, false);
						newItem.transform.SetAsLastSibling();
						newItem.gameObject.SetActive(true);

						// Store the item
						visibleLoadingItems.Add(newItem.id, newItem);
				}

				protected virtual void OnLoadingFinished(EventsChannel.Promise promise) {
						LoadingUIItem item;
						visibleLoadingItems.TryGetValue(promise.EventId, out item);

						if (item == null)
								return;

						// Remove the item
						visibleLoadingItems.Remove(promise.EventId);
						pool.Store(item);

						// if everything is done loading, we can disable the loading view
						if (visibleLoadingItems.Count == 0)
								gameObject.SetActive(false);
				}
		}
}