using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using TeamStor.Engine.Graphics;

namespace TeamStor.Engine
{
	/// <summary>
	/// Manages asset loading and unloading.
	/// </summary>
	public class AssetsManager : IDisposable
	{
		private struct LoadedAsset
		{
			public LoadedAsset(IDisposable asset, bool keepAfterStateChange)
			{
				Asset = asset;
				KeepAfterStateChange = keepAfterStateChange;
			}
			
			public IDisposable Asset;
			public bool KeepAfterStateChange;
		}
		
		private Dictionary<string, LoadedAsset> _loadedAssets = new Dictionary<string, LoadedAsset>();

		/// <summary>
		/// Game class.
		/// </summary>
		public Game Game
		{
			get;
			private set;
		}
		
		/// <summary>
		/// The directory to load assets from.
		/// </summary>
		public string Directory
		{
			get;
			private set;
		}

		/// <summary>
		/// Amount of loaded assets.
		/// </summary>
		public int LoadedAssets
		{
			get { return _loadedAssets.Count; }
		}
		
		/// <summary>
		/// Amount of state-specific loaded assets.
		/// </summary>
		public int StateLoadedAssets
		{
			get { return _loadedAssets.Count(asset => !asset.Value.KeepAfterStateChange); }
		}
		
		/// <param name="game">Game class.</param>
		/// <param name="dir">The directory to load assets from.</param>
		public AssetsManager(Game game, string dir)
		{
			Game = game;
			Directory = dir;

			Game.OnStateChange += OnStateChange;
		}

		public void Dispose()
		{
			foreach(KeyValuePair<string, LoadedAsset> asset in _loadedAssets)
				asset.Value.Asset.Dispose();
			
			_loadedAssets.Clear();
		}
		
		/// <summary>
		/// Loads or gets an asset with the specified name and type.
		/// </summary>
		/// <param name="name">The name of the asset.</param>
		/// <typeparam name="T">The type of the asset (can be Texture2D, Font, SoundEffect, Song or Effect)</typeparam>
		/// <returns>The asset</returns>
		public T Get<T>(string name, bool keepAfterStateChange = false) where T : class, IDisposable
		{
			T asset = null;
			if(TryLoadAsset<T>(name, out asset, keepAfterStateChange))
				return asset;
			
			throw new Exception("Asset with name \"" + name + "\" couldn't be loaded.");
		}

		/// <summary>
		/// Tries to load an asset with the specified name and type.
		/// </summary>
		/// <param name="name">The name of the asset.</param>
		/// <param name="asset">The returned asset, or null if it couldn't be loaded.</param>
		/// <typeparam name="T">The type of the asset (can be Texture2D, Font, SoundEffect, Song or Effect)</typeparam>
		/// <returns>true if the asset was loaded</returns>
		public bool TryLoadAsset<T>(string name, out T asset, bool keepAfterStateChange = false) where T : class, IDisposable
		{
			asset = null;

			if(_loadedAssets.ContainsKey(name.ToLowerInvariant()))
			{
				if(_loadedAssets[name.ToLowerInvariant()].Asset is T)
				{
					asset = (T)_loadedAssets[name.ToLowerInvariant()].Asset;
					return true;
				}

				return false;
			}
			
			if(!File.Exists(Directory + "/" + name))
				return false;

			try
			{
				if(typeof(T) == typeof(Texture2D))
				{
					using(FileStream stream = new FileStream(Directory + "/" + name, FileMode.Open))
					{
						asset = Texture2D.FromStream(Game.GraphicsDevice, stream) as T;
						_loadedAssets.Add(name.ToLowerInvariant(), new LoadedAsset(asset, keepAfterStateChange));
						return true;
					}
				}
				
				if(typeof(T) == typeof(Font))
				{
					asset = new Font(Game.GraphicsDevice, Directory + "/" + name) as T;
					_loadedAssets.Add(name.ToLowerInvariant(), new LoadedAsset(asset, keepAfterStateChange));
					return true;
				}
				
				if(typeof(T) == typeof(SoundEffect))
				{
					using(FileStream stream = new FileStream(Directory + "/" + name, FileMode.Open))
					{
						asset = SoundEffect.FromStream(stream) as T;
						_loadedAssets.Add(name.ToLowerInvariant(), new LoadedAsset(asset, keepAfterStateChange));
						return true;
					}
				}
				
				if(typeof(T) == typeof(Song))
				{
					asset = Song.FromUri(name, new Uri(Directory + "/" + name, UriKind.RelativeOrAbsolute)) as T;
					_loadedAssets.Add(name.ToLowerInvariant(), new LoadedAsset(asset, keepAfterStateChange));
					return true;
				}
				
				if(typeof(T) == typeof(Effect))
				{
					asset = new Effect(Game.GraphicsDevice, File.ReadAllBytes(Directory + "/" + name)) as T;
					_loadedAssets.Add(name.ToLowerInvariant(), new LoadedAsset(asset, keepAfterStateChange));
					return true;
				}

				return false;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Unloads an asset if it's loaded.
		/// </summary>
		/// <param name="name">The name of the asset.</param>
		/// <returns>True if an asset was actually unloaded.</returns>
		public bool UnloadAsset(string name)
		{
			if(!_loadedAssets.ContainsKey(name.ToLowerInvariant()))
				return false;

			_loadedAssets[name.ToLowerInvariant()].Asset.Dispose();
			return true;
		}

		/// <summary>
		/// If this asset manager has the specified asset loaded.
		/// </summary>
		/// <param name="name">The name of the asset.</param>
		/// <returns>If the asset is loaded.</returns>
		public bool HasAssetLoaded(string name)
		{
			return _loadedAssets.ContainsKey(name.ToLowerInvariant());
		}
		
		/// <summary>
		/// If the specified asset will be kept after a state change.
		/// </summary>
		/// <param name="name">The name of the asset.</param>
		/// <returns>If the asset is loaded and will be kept after a state change.</returns>
		public bool IsAssetKeptAfterStateChange(string name)
		{
			return _loadedAssets.ContainsKey(name.ToLowerInvariant()) && _loadedAssets[name.ToLowerInvariant()].KeepAfterStateChange;
		}
		
		private void OnStateChange(object sender, Game.ChangeStateEventArgs e)
		{
			if(e.From != e.To)
			{
				List<string> toRemove = new List<string>();

				foreach(KeyValuePair<string, LoadedAsset> asset in _loadedAssets)
				{
					if(!asset.Value.KeepAfterStateChange)
					{
						asset.Value.Asset.Dispose();
						toRemove.Add(asset.Key);
					}
				}

				foreach(string s in toRemove)
					_loadedAssets.Remove(s);
			}
		}
	}
}