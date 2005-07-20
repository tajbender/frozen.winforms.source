//	<file>
//		<copyright see="www.electrifier.org"/>
//		<license   see="www.electrifier.org"/>
//		<owner     name="Thorsten Jung" email="taj@electrifier.org"/>
//		<version   value="$Id: ShellTreeView.cs,v 1.19 2004/09/10 20:30:34 taj bender Exp $"/>
//	</file>

using System;
using System.Windows.Forms;

using Electrifier.Core.Controls;
using Electrifier.Core.Services;
using Electrifier.Core.Shell32.Services;
using Electrifier.Win32API;

namespace Electrifier.Core.Shell32.Controls {
	/// <summary>
	/// Zusammenfassung f�r ShellTreeView.
	/// </summary>
	public class ShellTreeView : ExtTreeView {
		protected static IconManager                 iconManager = (IconManager)ServiceManager.Services.GetService(typeof(IconManager));
		protected        ShellTreeViewNode           rootNode    = null;
		protected new    ShellTreeViewNodeCollection nodes       = null;
		public    new    ShellTreeViewNodeCollection Nodes       { get { return nodes; } }

		public    new    ShellTreeViewNode           SelectedNode {
			get { return base.SelectedNode as ShellTreeViewNode; }
			set { base.SelectedNode = value; }
		}

		public ShellTreeView(ShellAPI.CSIDL shellObjectCSIDL)
			: this(PIDLManager.CreateFromCSIDL(shellObjectCSIDL), true) {}

		public ShellTreeView(string shellObjectFullPath)
			: this(PIDLManager.CreateFromPathW(shellObjectFullPath), true) {}

		public ShellTreeView(IntPtr shellObjectPIDL)
			: this(shellObjectPIDL, false) {}

		public ShellTreeView(IntPtr pidl, bool pidlSelfCreated) : base() {
			// Initialize underlying ExtTreeView-component
			this.rootNode        = new ShellTreeViewNode(pidl, pidlSelfCreated);
			this.nodes           = new ShellTreeViewNodeCollection(base.Nodes);
			this.SystemImageList = iconManager.SmallImageList;
			this.HideSelection   = false;
			this.ShowRootLines   = false;

			this.Nodes.Add(rootNode);
			this.rootNode.Expand();
			if(this.rootNode.FirstNode != null) {
				this.rootNode.FirstNode.Expand();
				this.SelectedNode = rootNode.FirstNode;
			}

			// Create a file info thread to gather visual info for root item
			IconManager.FileInfoThread fileInfoThread = new IconManager.FileInfoThread(rootNode);

			this.BorderStyle = BorderStyle.None;
		}

		public ShellTreeViewNode FindNodeByPIDL(IntPtr shellObjectPIDL) {
			this.BeginUpdate();

			try {

				// First of all, test whether the given PIDL anyhow derives from our root node
				if(PIDLManager.IsParent(this.rootNode.AbsolutePIDL, shellObjectPIDL, false)) {
					// If we have luck, just the root node itself is requested
					if(PIDLManager.IsEqual(this.rootNode.AbsolutePIDL, shellObjectPIDL))
						return this.rootNode;

					ShellTreeViewNode actNode = this.rootNode.FirstNode;
					do {
						if(PIDLManager.IsEqual(actNode.AbsolutePIDL, shellObjectPIDL))
							return actNode;
						if(PIDLManager.IsParent(actNode.AbsolutePIDL, shellObjectPIDL, false)) {
							if(actNode.Nodes.Count == 0)
								actNode.Expand();

							return actNode.FindChildNodeByPIDL(shellObjectPIDL);
						}
					} while((actNode = actNode.NextNode) != null);
				}
			} finally {
				this.EndUpdate();
			}
			
			return null;
		}

	}
}
