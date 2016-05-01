﻿using NLog;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Windows.Forms;
using WindowsFirewallCore;
using WindowsFirewallCore.IPCommunication.Interfaces;

namespace WindowsFirewallShellExtension
{
	#pragma warning disable CC0022 // Should dispose object
	[ComVisible(true)]
	[COMServerAssociation(AssociationType.ClassOfExtension, ".exe")]
	public class WindowsFirewallShellExtension : SharpContextMenu
	{
		private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

		private ChannelFactory<IShellService> shellServiceChannelFactory { get; set; }
		private IShellService shellService { get; set; }
		private ICommunicationObject shellServiceChannel
		{
			get
			{
				return (ICommunicationObject)shellService;
			}
		}

		protected override bool CanShowMenu()
		{
			return true;
		}

		protected override ContextMenuStrip CreateMenu()
		{
			LOG.Debug("Create context menu");
			try
			{
				var menu = new ContextMenuStrip();

				#region WindowsFirewallDashboard Submenu
				var itemFirewallDashboard = new ToolStripMenuItem
				{
					Text = "Windows Firewall Dashboard"
				};
				menu.Items.Add(itemFirewallDashboard);

				#region Submenu items
				var itemStatusApplicationRule = new ToolStripMenuItem
				{
					Text = "Application status"
				};

				var itemAddApplicationRule = new ToolStripMenuItem
				{
					Text = "Add application rule"
				};
				itemAddApplicationRule.Click += AddApplicationRule;

				var itemRemoveApplicationRule = new ToolStripMenuItem
				{
					Text = "Remove application rule"
				};
				itemRemoveApplicationRule.Click += RemoveApplicationRule;
				#endregion

				// application status (later implementation)
				if (SelectedItemPaths.Count() == 1)
				{
					//itemFirewallDashboard.DropDownItems.Add(itemStatusApplicationRule);
				}

				itemFirewallDashboard.DropDownItems.Add(itemAddApplicationRule);
				itemFirewallDashboard.DropDownItems.Add(itemRemoveApplicationRule);

				#endregion

				return menu;
			}
			catch (Exception ex)
			{
				LOG.Error(ex.Message);
			}

			return null;
		}

		private void AddApplicationRule(object sender, EventArgs e)
		{
			try
			{
				var shellService = OpenChannel();
				shellService.AddApplicationRule(SelectedItemPaths);
				CloseChannel();
			}
			catch (Exception ex)
			{
				LOG.Error(ex.Message);
				MessageBox.Show(ex.Message);
			}
		}

		private void RemoveApplicationRule(object sender, EventArgs e)
		{
			try
			{
				var shellService = OpenChannel();
				shellService.RemoveApplicationRule(SelectedItemPaths);
				CloseChannel();
			}
			catch (Exception ex)
			{
				LOG.Error(ex.Message);
				MessageBox.Show(ex.Message);
			}
		}

		private IShellService OpenChannel()
		{
			LOG.Debug("Open channel");

			if (shellServiceChannelFactory == null)
			{
				shellServiceChannelFactory = new ChannelFactory<IShellService>(new NetNamedPipeBinding(), new EndpointAddress(CoreConstants.PipeEndpoint));
			}

			if (shellService == null)
			{
				shellService = shellServiceChannelFactory.CreateChannel();
			}

			LOG.Debug("Channel opened");

			return shellService;
		}

		private void CloseChannel()
		{
			try
			{
				LOG.Debug("Close channel");

				shellServiceChannel.Close();
				shellServiceChannelFactory.Close();

				LOG.Debug("Channel closed");
			}
			catch (Exception ex)
			{
				LOG.Error(ex.Message);
				MessageBox.Show(ex.Message);
			}
			finally
			{
				shellServiceChannel.Abort();
				shellServiceChannelFactory.Abort();
			}
		}
	}
}
