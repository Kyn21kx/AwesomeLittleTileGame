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
		int byteIndex = 0, bitIndex = 0;
		//Texture2D encryptedImage = new Texture2D(image.width, image.height, image.format, false);
		for (int y = 0; y < image.height; y++) {
			for (int x = 0; x < image.width; x++) {
				if (byteIndex >= message.Length) break;
				//Split the byte into its bit components
				byte[] currentByte = SplitByte((byte)message[byteIndex]);
				//Apply a bitwise or to each pixel
				Color originalPixel = image.GetPixel(x, y);
				//Create a new pixel modifying the blue channel
				byte originalBlue = (byte)(originalPixel.b * 255);
				byte n_blue = (byte)(originalBlue | currentByte[bitIndex]);
				Debug.Log($"Original blue: {originalBlue}, message byte: {currentByte[bitIndex]}, encrypted blue: {n_blue}");
				Color encryptedColor = new Color(originalPixel.r, originalPixel.g, n_blue / 255f, originalPixel.a);
				image.SetPixel(x, y, encryptedColor);
				bitIndex++;
				if (bitIndex >= 8) {
					bitIndex = 0;
					byteIndex++;
				}
			}
		}

		image.Apply();
		byte[] data = image.EncodeToPNG();
		File.WriteAllBytes($"./Assets/Sprites/Encoded_{image.name}.png", data);
	}

	private byte[] SplitByte(byte b) {
		byte[] result = new byte[8];
		byte controlByte = 1;
		for (int i = 0; i < 8; i++) {
			result[i] = (byte)((b & controlByte) >> i);
			controlByte <<= 1;
		}
		System.Array.Reverse(result);
		return result;
	}

	public string Decrypt(Texture2D image) {

		return null;
	}

}
