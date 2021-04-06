using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Steganography : MonoBehaviour {

	public Texture2D image;

	private void Start() {
		Encrypt(image, "Test");
	}

	public void Encrypt(Texture2D image, string message) {
		int bitIndex = 0;
		Texture2D encryptedImage = new Texture2D(image.width, image.height, image.format, false);
		for (int y = 0; y < image.height; y++) {
			for (int x = 0; x < image.width; x++) {
				if (bitIndex >= message.Length) break;

				//Apply a bitwise or to each pixel
				Color originalPixel = image.GetPixel(x, y);
				//Create a new pixel modifying the blue channel
				byte originalBlue = (byte)(originalPixel.b * 255);
				byte n_blue = (byte)(originalBlue | (byte)message[bitIndex]);
				Debug.Log($"Original blue: {(byte)originalPixel.b}, message byte: {(byte)message[bitIndex]}, encrypted blue: {n_blue}");
				Color encryptedColor = new Color(originalPixel.r, originalPixel.g, n_blue / 255f, originalPixel.a);
				encryptedImage.SetPixel(x, y, encryptedColor);
				bitIndex++;
			}
		}

		byte[] data = encryptedImage.EncodeToJPG();
		File.WriteAllBytes($"./Assets/Sprites/Encoded_{image.name}.jpg", data);
	}

	private BitArray SplitByte(byte b) {
		//BitArray bits = new BitArray()
		return null;
	}

	public string Decrypt(Texture2D image) {
		return null;
	}

}
