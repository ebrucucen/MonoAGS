﻿using System;
using NUnit.Framework;
using AGS.Engine;
using System.Threading.Tasks;
using AGS.API;

namespace Tests
{
	[TestFixture ()]
	public class EventTests
	{
		private int syncEvents, asyncEvents;
		const int x = 10;

		[SetUp]
		public void Init()
		{
			syncEvents = 0;
			asyncEvents = 0;
		}

		[Test ()]
		public void NoSubscribersTest ()
		{
			AGSEvent<AGSEventArgs> ev = new AGSEvent<AGSEventArgs> ();
			ev.Invoke (this, null);
			Assert.AreEqual (0, syncEvents);
			Assert.AreEqual (0, asyncEvents);
		}

		[Test ()]
		public async Task NoSubscribersAsyncTest ()
		{
			AGSEvent<AGSEventArgs> ev = new AGSEvent<AGSEventArgs> ();
			await ev.InvokeAsync (this, null);
			Assert.AreEqual (0, syncEvents);
			Assert.AreEqual (0, asyncEvents);
		}

		[Test ()]
		public void SingleSubscriberTest ()
		{
			AGSEvent<MockEventArgs> ev = new AGSEvent<MockEventArgs> ();
			ev.Subscribe (onEvent);
			ev.Invoke (this, new MockEventArgs(x));
			Assert.AreEqual (1, syncEvents);
			Assert.AreEqual (0, asyncEvents);
		}

		[Test ()]
		public void SingleAsyncSubscriberTest ()
		{
			AGSEvent<MockEventArgs> ev = new AGSEvent<MockEventArgs> ();
			ev.SubscribeToAsync (onEventAsync);
			ev.Invoke (this, new MockEventArgs(x));
			Assert.AreEqual (0, syncEvents);
			Assert.AreEqual (1, asyncEvents);
		}

		[Test ()]
		public async Task SingleSubscriberAsyncTest ()
		{
			AGSEvent<MockEventArgs> ev = new AGSEvent<MockEventArgs> ();
			ev.Subscribe (onEvent);
			await ev.InvokeAsync (this, new MockEventArgs(x));
			Assert.AreEqual (1, syncEvents);
			Assert.AreEqual (0, asyncEvents);
		}

		[Test ()]
		public async Task SingleAsyncSubscriberAsyncTest ()
		{
			AGSEvent<MockEventArgs> ev = new AGSEvent<MockEventArgs> ();
			ev.SubscribeToAsync (onEventAsync);
			await ev.InvokeAsync (this, new MockEventArgs(x));
			Assert.AreEqual (0, syncEvents);
			Assert.AreEqual (1, asyncEvents);
		}

		[Test ()]
		public void MultipleSubscribersTest ()
		{
			AGSEvent<MockEventArgs> ev = new AGSEvent<MockEventArgs> ();
			ev.Subscribe (onEvent);
			ev.Subscribe ((sender, e) => onEvent(sender, e));
			ev.SubscribeToAsync (onEventAsync);
			ev.SubscribeToAsync (async (sender, e) => await onEventAsync(sender, e));
			ev.Invoke (this, new MockEventArgs(x));
			Assert.AreEqual (2, syncEvents);
			Assert.AreEqual (2, asyncEvents);
		}

		[Test ()]
		public async Task MultipleSubscribersAsyncTest ()
		{
			AGSEvent<MockEventArgs> ev = new AGSEvent<MockEventArgs> ();
			ev.Subscribe (onEvent);
			ev.Subscribe ((sender, e) => onEvent(sender, e));
			ev.SubscribeToAsync (onEventAsync);
			ev.SubscribeToAsync (async (sender, e) => await onEventAsync(sender, e));
			await ev.InvokeAsync (this, new MockEventArgs(x));
			Assert.AreEqual (2, syncEvents);
			Assert.AreEqual (2, asyncEvents);
		}

		[Test]
		public void SubscribeUnsubscribeTest()
		{
			AGSEvent<MockEventArgs> ev = new AGSEvent<MockEventArgs> ();
			ev.Subscribe(onEvent);
			ev.Unsubscribe(onEvent);
			Assert.AreEqual(0, ev.SubscribersCount);
		}

		[Test]
		public void SubscribeUnsubscribeOnDifferentTargetTest()
		{
			AGSEvent<MockEventArgs> ev = new AGSEvent<MockEventArgs> ();
			ev.Subscribe(onEvent);
			EventTests target2 = new EventTests ();
			ev.Unsubscribe(target2.onEvent);
			Assert.AreEqual(1, ev.SubscribersCount);
		}

		[Test]
		public void SubscribeUnsubscribeAsyncTest()
		{
			AGSEvent<MockEventArgs> ev = new AGSEvent<MockEventArgs> ();
			ev.SubscribeToAsync(onEventAsync);
			ev.UnsubscribeToAsync(onEventAsync);
			Assert.AreEqual(0, ev.SubscribersCount);
		}

		[Test]
		public void SubscribeUnsubscribeOnDifferentTargetAsyncTest()
		{
			AGSEvent<MockEventArgs> ev = new AGSEvent<MockEventArgs> ();
			ev.SubscribeToAsync(onEventAsync);
			EventTests target2 = new EventTests ();
			ev.UnsubscribeToAsync(target2.onEventAsync);
			Assert.AreEqual(1, ev.SubscribersCount);
		}

		private void onEvent(object sender, MockEventArgs e)
		{
			Assert.AreEqual (x, e.X);
			syncEvents++;
		}

		private async Task onEventAsync(object sender, MockEventArgs e)
		{
			await Task.Delay (1);
			Assert.AreEqual (x, e.X);
			asyncEvents++;
		}

		private class MockEventArgs : AGSEventArgs
		{
			public MockEventArgs(int x)
			{
				X = x;
			}

			public int X { get; private set; }
		}
	}
}

