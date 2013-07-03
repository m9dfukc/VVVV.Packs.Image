﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;

namespace VVVV.Nodes.DeckLink
{
	/// <summary>
	/// A method for copying data back from a DX9Ex shared Surface on the GPU
	/// </summary>
	class ReadTexture : IDisposable
	{
		struct Vertex
		{
			public Vector4 Position;
			public Vector2 TextureCoord;

			static public int GetSize()
			{
				return Vector4.SizeInBytes + Vector2.SizeInBytes;
			}
		}

		Direct3DEx FContext;
		DeviceEx FDevice;
		Control FHiddenControl;
		bool FInitialised = false;

		int FWidth;
		int FHeight;
		IntPtr FHandle;
		Format FFormat;
		Usage FUsage;

		/*
		VertexBuffer FVertices;
		VertexElement[] FVertexElements;
		VertexDeclaration FVertexDeclaration;
		*/

		Texture FTextureShared;
		Texture FTextureCopied;
		Surface FSurfaceOffscreen;

		public ReadTexture(int width, int height, uint handle, Format format, Usage usage)
		{
			this.FWidth = width;
			this.FHeight = height;
			this.FHandle = (IntPtr)unchecked((int)handle);
			this.FFormat = format;
			this.FUsage = usage;

			Initialise();
		}

		public ReadTexture(int width, int height, IntPtr handle, Format format, Usage usage)
		{
			this.FWidth = width;
			this.FHeight = height;
			this.FHandle = handle;
			this.FFormat = format;
			this.FUsage = usage;

			Initialise();
		}

		void Initialise()
		{
			if (this.FHandle == (IntPtr) 0)
				throw (new Exception("No shared texture handle set"));
			this.FContext = new Direct3DEx();

			this.FHiddenControl = new Control();
			this.FHiddenControl.Visible = false;
			this.FHiddenControl.Width = this.FWidth;
			this.FHiddenControl.Height = this.FHeight;
			
			var flags = CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.PureDevice | CreateFlags.FpuPreserve;
			this.FDevice = new DeviceEx(FContext, 0, DeviceType.Hardware, this.FHiddenControl.Handle, flags, new PresentParameters()
			{
				BackBufferWidth = this.FWidth,
				BackBufferHeight = this.FHeight
			});

			/*
			this.FVertices = new VertexBuffer(FDevice, Vertex.GetSize(), Usage.WriteOnly, VertexFormat.None, pool);
			this.FVertices.Lock(0, 0, LockFlags.None).WriteRange(new[] {
                new Vertex() { Position = new Vector4(0, 0, 0.5f, 1.0f), TextureCoord = new Vector2(0,0) },
                new Vertex() { Position = new Vector4(width, 0, 0.5f, 1.0f), TextureCoord = new Vector2(1,0) },
                new Vertex() { Position = new Vector4(0, height, 0.5f, 1.0f), TextureCoord = new Vector2(0,1) },
                new Vertex() { Position = new Vector4(width, height, 0.5f, 1.0f), TextureCoord = new Vector2(1,1) }
			});
			this.FVertices.Unlock();

			this.FVertexElements = new[] {
				new VertexElement(0, 0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
				new VertexElement(0, 16 + 4, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
				VertexElement.VertexDeclarationEnd
			};

			this.FVertexDeclaration = new VertexDeclaration(FDevice, FVertexElements);

			FDevice.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.None);
			FDevice.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
			FDevice.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.Linear);
			*/

			this.FTextureShared = new Texture(this.FDevice, this.FWidth, this.FHeight, 1, FUsage, FFormat, Pool.Default, ref this.FHandle);
			this.FTextureCopied = new Texture(this.FDevice, this.FWidth, this.FHeight, 1, Usage.None, FFormat, Pool.Default);
			this.FSurfaceOffscreen = Surface.CreateOffscreenPlainEx(FDevice, FWidth, FHeight, FFormat, Pool.SystemMemory, Usage.None);

			this.FInitialised = true;
		}

		/// <summary>
		/// Read back the data from the texture into a CPU buffer
		/// </summary>
		/// <param name="buffer"></param>
		public void ReadBack(byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = unchecked((byte) i);
			}
			return;
			FDevice.BeginScene();
			try
			{
				FDevice.StretchRectangle(FTextureShared.GetSurfaceLevel(0), FTextureCopied.GetSurfaceLevel(0), TextureFilter.None);
				FDevice.GetRenderTargetData(FTextureCopied.GetSurfaceLevel(0), FSurfaceOffscreen);

				var rect = FSurfaceOffscreen.LockRectangle(LockFlags.ReadOnly);
				try
				{
					rect.Data.Read(buffer, 0, buffer.Length);
				}
				catch (Exception e)
				{
					FSurfaceOffscreen.UnlockRectangle();
					throw;
				}
				FDevice.EndScene();
			}
			catch (Exception e)
			{
				FDevice.EndScene();
				throw;
			}
		}

		public int BufferLength
		{
			get
			{
				return this.FWidth * this.FHeight * 4;
			}
		}

		public void Dispose()
		{
			FTextureShared.Dispose();
			FTextureCopied.Dispose();
			FSurfaceOffscreen.Dispose(); 
			
			FContext.Dispose();
			FDevice.Dispose();
			FHiddenControl.Dispose();
		}
	}
}
