using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using TeamStor.Engine.Graphics;
using Microsoft.Xna.Framework;
using TeamStor.Engine.Graphics;
using MonoGame.Utilities.Png;

namespace TeamStor.Engine
{
	/// <summary>
	/// Manages asset loading and unloading.
	/// </summary>
	public class AssetsManager : IDisposable
	{
		private struct LoadedAsset
		{
			public LoadedAsset(IDisposable asset, string path, bool keepAfterStateChange)
			{
				Asset = asset;
                Path = path;
				KeepAfterStateChange = keepAfterStateChange;
                TrackedFile = null;
			}
			
			public IDisposable Asset;
            public string Path;
			public bool KeepAfterStateChange;

            public string TrackedFile;
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
		public AssetsManager(Game game)
		{
			Game = game;
		}

        public void Dispose()
		{
            foreach(KeyValuePair<string, LoadedAsset> asset in _loadedAssets)
            {
                asset.Value.Asset.Dispose();
                if(!string.IsNullOrEmpty(asset.Value.TrackedFile))
                    File.Delete(asset.Value.TrackedFile);
            }
			
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
            Exception errorReason;
			if(TryLoadAsset<T>(name, out asset, out errorReason, keepAfterStateChange))
				return asset;
			
			throw new Exception("Asset with name \"" + name + "\" couldn't be loaded (" + errorReason + ").", errorReason);
		}

		/// <summary>
		/// Tries to load an asset with the specified name and type.
		/// </summary>
		/// <param name="name">The name of the asset.</param>
		/// <param name="asset">The returned asset, or null if it couldn't be loaded.</param>
		/// <typeparam name="T">The type of the asset (can be Texture2D, Font, SoundEffect, Song or Effect)</typeparam>
		/// <returns>true if the asset was loaded</returns>
		public bool TryLoadAsset<T>(string name, out T asset, out Exception errorReason, bool keepAfterStateChange = false) where T : class, IDisposable
		{
            asset = null;
            errorReason = null;

            if(_loadedAssets.ContainsKey(name))
			{
				if(_loadedAssets[name].Asset is T)
				{
					asset = (T)_loadedAssets[name].Asset;
					return true;
				}

				return false;
			}
			
			try
			{
				if(typeof(T) == typeof(Texture2D))
				{
					using(Stream stream = Game.Activity.Assets.Open(name))
					{
                        MemoryStream copiedStream = new MemoryStream();
                        stream.CopyTo(copiedStream);

                        PngReader reader = new PngReader();
                        Texture2D texture = reader.Read(copiedStream, Game.GraphicsDevice);
                        Color[] data = new Color[texture.Width * texture.Height];
                        texture.GetData(data);

                        for(int i = 0; i < data.Length; i++)
                            data[i] = Color.FromNonPremultiplied(data[i].ToVector4());

                        Texture2D newTexture = new Texture2D(Game.GraphicsDevice, texture.Width, texture.Height);
                        newTexture.SetData(data);
                        texture.Dispose();

                        asset = newTexture as T;
						_loadedAssets.Add(name, new LoadedAsset(asset, name, keepAfterStateChange));
						return true;
					}
				}


                if(typeof(T) == typeof(Font))
				{
                    using(Stream stream = Game.Activity.Assets.Open(name))
                    {
                        asset = new Font(Game.GraphicsDevice, stream) as T;
                        _loadedAssets.Add(name, new LoadedAsset(asset, name, keepAfterStateChange));
                        return true;
                    }
				}

				
				if(typeof(T) == typeof(SoundEffect))
				{
					using(Stream stream = Game.Activity.Assets.Open(name))
					{
						asset = SoundEffect.FromStream(stream) as T;
						_loadedAssets.Add(name, new LoadedAsset(asset, name, keepAfterStateChange));
						return true;
					}
				}
				
				if(typeof(T) == typeof(Song))
				{
                    // https://stackoverflow.com/questions/5813657/xna-4-song-fromuri-containing-spaces/5829463#5829463
                    ConstructorInfo ctor = typeof(Song).GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new[] { typeof(string) }, null);
                    asset = ctor.Invoke(new object[] { name }) as T;

                    LoadedAsset a = new LoadedAsset(asset, name, keepAfterStateChange);
                    _loadedAssets.Add(name, a);
                    return true;
				}
				
				if(typeof(T) == typeof(Effect))
				{
                    using(Stream stream = Game.Activity.Assets.Open(name))
                    {
                        MemoryStream s = new MemoryStream();
                        stream.CopyTo(s);
                        asset = new Effect(Game.GraphicsDevice, s.ToArray()) as T;
                        _loadedAssets.Add(name, new LoadedAsset(asset, name, keepAfterStateChange));
                        return true;
                    }
				}

				return false;
			}
			catch(Exception e)
			{
                errorReason = e;
                return false;
			}
		}

        /// <summary>
        /// Reloads an asset if it's loaded.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        /// <returns>True if an asset was actually reloaded.</returns>
        public bool ReloadAsset(string name)
        {
            if(!_loadedAssets.ContainsKey(name.ToLowerInvariant()))
                return false;

            LoadedAsset oldAsset = _loadedAssets[name.ToLowerInvariant()];
            UnloadAsset(name);

            Texture2D o1;

            Font o2;

            SoundEffect o3;
            Song o4;
            Effect o5;

            Exception errorReason;

            if(oldAsset.Asset is Texture2D)
                return TryLoadAsset<Texture2D>(name, out o1, out errorReason, oldAsset.KeepAfterStateChange);
            if(oldAsset.Asset is Font)
                return TryLoadAsset<Font>(name, out o2, out errorReason, oldAsset.KeepAfterStateChange);
            if(oldAsset.Asset is SoundEffect)
                return TryLoadAsset<SoundEffect>(name, out o3, out errorReason, oldAsset.KeepAfterStateChange);
            if(oldAsset.Asset is Song)
                return TryLoadAsset<Song>(name, out o4, out errorReason, oldAsset.KeepAfterStateChange);
            if(oldAsset.Asset is Effect)
                return TryLoadAsset<Effect>(name, out o5, out errorReason, oldAsset.KeepAfterStateChange);
            return false;
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
            if(!string.IsNullOrEmpty(_loadedAssets[name.ToLowerInvariant()].TrackedFile))
                File.Delete(_loadedAssets[name.ToLowerInvariant()].TrackedFile);
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
                        if(!string.IsNullOrEmpty(asset.Value.TrackedFile))
                            File.Delete(asset.Value.TrackedFile);
                        toRemove.Add(asset.Key);
					}
				}

				foreach(string s in toRemove)
					_loadedAssets.Remove(s);
			}
		}
	}
}