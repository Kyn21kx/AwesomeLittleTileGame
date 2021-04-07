using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEngine.Assertions;

public class Steganography : MonoBehaviour {

	public Texture2D[] images;
	public string[] hints;
	
	private void Start() {
		/*
		for (int i = 0; i < images.Length; i++) {
			Encrypt(images[i], hints[i]);
			Debug.Log($"{images[i].name}: {Decrypt(images[i])}");
		}*/
	}

	public void Encrypt(Texture2D image, string message) {
		int byteIndex = 0, bitIndex = 0;
		message = message.Length.ToString("X") + message;
		byte[] currentByte = SplitByte((byte)message[byteIndex]);
		//Texture2D encryptedImage = new Texture2D(image.width, image.height, image.format, false);
		for (int y = 0; y < image.height; y++) {
			for (int x = 0; x < image.width; x++) {
				Color originalPixel = image.GetPixel(x, y);
				if (byteIndex >= message.Length) {
					//Create a control byte where one pixel's blue is 255
					originalPixel.b = 1f;
					image.SetPixel(x, y, originalPixel);
					goto WRITING;
				}
				//Split the byte into its bit components
				//Apply a bitwise or to each pixel
				//Create a new pixel modifying the blue channel
				byte originalBlue = (byte)(originalPixel.b * 255);
				byte[] allBits = SplitByte(originalBlue);
				allBits[7] = currentByte[bitIndex];
				System.Array.Reverse(allBits);
				byte n_blue;
				JoinBits(allBits, out n_blue);
				//Debug.Log($"Original blue: {originalBlue}, message byte: {currentByte[bitIndex]}, encrypted blue: {n_blue}, encoded: {n_blue & 1}");
				Assert.AreEqual(currentByte[bitIndex], n_blue & 1);
				Color encryptedColor = new Color(originalPixel.r, originalPixel.g, n_blue / 255f, originalPixel.a);
				image.SetPixel(x, y, encryptedColor);
				bitIndex++;
				if (bitIndex >= 8) {
					bitIndex = 0;
					byteIndex++;
					if (byteIndex < message.Length) {
						currentByte = SplitByte((byte)message[byteIndex]);
					}
				}
			}
		}
		WRITING:
		image.Apply();
		byte[] data = image.EncodeToPNG();
		File.WriteAllBytes($"./Assets/Sprites/{image.name}.png", data);
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

	private void JoinBits(byte[] b, out byte result) {
		result = 0;
		for (int i = 0; i < b.Length; i++) {
			//Add the bit and shift it
			result |= (byte)(b[i] << i);
		}
	}

	private int ConvertFromHex(string hex) {
		return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
	}

	public string Decrypt(Texture2D image) {
		StringBuilder sb = new StringBuilder();
		int bitIndex = 0;
		byte[] bytes = new byte[8];
		string length = "";
		int characterIndex = -1;
		for (int y = 0; y < image.height; y++) {
			for (int x = 0; x < image.width; x++) {
				if (bitIndex >= 8) {
					byte result;
					System.Array.Reverse(bytes);
					JoinBits(bytes, out result);
					bitIndex = 0;
					bytes = new byte[8];

					characterIndex++;

					if (characterIndex < 2) {
						length += ((char)result).ToString();
					}
					else
						sb.Append((char)result);
				}
				Color pixel = image.GetPixel(x, y);
				byte blue = (byte)(pixel.b * 255);
				//Get the last bit of the blue channel
				bytes[bitIndex] = (byte)(blue & 0x01);


				if (characterIndex > 1) {
					int realLength = ConvertFromHex(length.ToString());

					if (characterIndex >= realLength)
						return sb.ToString();
				}
				bitIndex++;
			}
		}
		return null;
	}

}
