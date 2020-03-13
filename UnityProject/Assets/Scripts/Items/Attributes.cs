﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IngameDebugConsole;
using UnityEngine;
using Mirror;
using UnityEngine.Serialization;
using Random = System.Random;


[RequireComponent(typeof(Integrity))]
[RequireComponent(typeof(CustomNetTransform))]
public class Attributes : NetworkBehaviour, IRightClickable, IServerSpawn, IExaminable
{

	[Tooltip("Display name of this item when spawned.")]
	[SerializeField]
	private string initialName;

	[SyncVar(hook = nameof(SyncArticleName))]
	private string articleName;
	/// <summary>
	/// Current name
	/// </summary>
	public string ArticleName => articleName;

	public string InitialName => initialName;

	[Tooltip("Description of this item when spawned.")]
	[SerializeField]
	private string initialDescription;

	[Tooltip("Will this item highlight on mouseover?")]
	[SerializeField]
	private bool willHighlight = true;

	[Tooltip("How much does one of these sell for when shipped on the cargo shuttle?")]
	[SerializeField]
	private int exportCost = 0;
	public int ExportCost
	{
		get
		{
			var stackable = GetComponent<Stackable>();

			if (stackable != null)
			{
				return exportCost * stackable.Amount;
			}

			return exportCost;
		}

	}

	[Tooltip("Should an alternate name be used when displaying this in the cargo console report?")]
	[SerializeField]
	private string exportName = null;
	public string ExportName => exportName;

	[Tooltip("Additional message to display in the cargo console report.")]
	[SerializeField]
	private string exportMessage = null;
	public string ExportMessage => exportMessage;

	[SyncVar(hook = nameof(SyncArticleDescription))]
	private string articleDescription;

	/// <summary>
	/// Current description
	/// </summary>
	public string ArticleDescription => articleDescription;

	public override void OnStartClient()
	{
		SyncArticleName(articleName, articleName);
		SyncArticleDescription(articleDescription, articleDescription);
		base.OnStartClient();
	}


	public virtual void OnSpawnServer(SpawnInfo info)
	{
		SyncArticleName(articleName, initialName);
		SyncArticleDescription(articleDescription, initialDescription);
	}

	private void SyncArticleName(string oldName, string newName)
	{
		articleName = newName;
	}

	private void SyncArticleDescription(string oldDescription, string newDescription)
	{
		articleDescription = newDescription;
	}

	/// <summary>
	/// When hovering over an object or item its name and description is shown as a tooltip on the bottom-left of the screen.
	/// The first letter of the object's name is always capitalized if the object has been given a name.
	/// If there is description it is shown in parentheses.
	/// </summary>
	public void OnHoverStart()
	{
		if(willHighlight)
		{
			Highlight.HighlightThis(gameObject);
		}
		string displayName = null;
		if (string.IsNullOrWhiteSpace(articleName))
		{
			displayName = gameObject.ExpensiveName();
		}
		else
		{
			displayName = articleName;
		}
		//failsafe
		if (string.IsNullOrWhiteSpace(displayName)) displayName = "error";

		UIManager.SetToolTip =
			displayName.First().ToString().ToUpper() + displayName.Substring(1) +
			(string.IsNullOrEmpty(articleDescription) ? "" : $" ({ articleDescription })");
	}

	public void OnHoverEnd()
	{
		Highlight.DeHighlight();

		UIManager.SetToolTip = string.Empty;
	}


	// Sends examine event to all monobehaviors on gameobject - keep for now - TODO: integrate w shift examine
	public void SendExamine()
	{
		SendMessage("OnExamine");
	}

	private void OnExamine()
	{
		RequestExamineMessage.Send(GetComponent<NetworkIdentity>().netId);
	}

	// Initial implementation of shift examine behaviour
	public string Examine(Vector3 worldPos)
	{
		string displayName = "<error>";
		if (string.IsNullOrWhiteSpace(articleName))
		{
			displayName = gameObject.ExpensiveName();
		}
		else
		{
			displayName = articleName;
		}

		string str = "This is a " + displayName + ".";

		if (!string.IsNullOrEmpty(initialDescription))
		{
			str = str + " " + initialDescription;
		}
		return str;
	}

	public RightClickableResult GenerateRightClickOptions()
	{
		return RightClickableResult.Create()
			.AddElement("Examine", OnExamine);
	}


	public void ServerSetArticleName(string newName)
	{
		SyncArticleName(articleName, newName);
	}

	[Server]
	public void ServerSetArticleDescription(string desc)
	{
		SyncArticleDescription(articleDescription, desc);
	}

}
