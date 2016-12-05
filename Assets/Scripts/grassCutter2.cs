using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class grassCutter2 : MonoBehaviour {
	float Radius = 1000f;
	public LayerMask layerMask;
	public terrainControl tc;

	protected int[,] BackupMap;
	public Terrain te;
	public GameObject mower;
	Vector3 pos;
	bool mowerOn = false;

	public Text roughText;
	private float count;
	private float baseRough = 0f;

	// Use this for initialization
	void Start () {

		CreateBackup(te);

		count = 0f;
		baseRough = (float)DetailMapCutoff (te, 0, 1);
		roughText.text = "Rough Cut: 0%";
		Debug.Log (baseRough);
	}

	void Update()
	{

		terrainControl.terrainArea ta;
		if (Input.GetKeyDown ("s")) {
			mowerOn = !mowerOn;
		}

		Debug.Log (mowerOn);


		pos = mower.transform.position;
		if (mowerOn == true) {
			CutGrass (te, pos, 4.0f);
		}
	}


	public void CutGrass(Terrain t, Vector3 position, float radius)
	{


		int TerrainDetailMapSize = t.terrainData.detailResolution;
		if(t.terrainData.size.x != t.terrainData.size.z)
		{
			Debug.Log ("X and Y Size of terrain have to be the same (RemoveGrass.CS Line 43)");
			return;
		}

		float PrPxSize = TerrainDetailMapSize / t.terrainData.size.x;

		Vector3 TexturePoint3D = position - t.transform.position;
		TexturePoint3D = TexturePoint3D * PrPxSize;

		//Debug.Log(TexturePoint3D);

		float[] xymaxmin = new float[4];
		xymaxmin[0] = TexturePoint3D.z + radius;
		xymaxmin[1] = TexturePoint3D.z - radius;
		xymaxmin[2] = TexturePoint3D.x + radius;
		xymaxmin[3] = TexturePoint3D.x - radius;


		int[,] map = t.terrainData.GetDetailLayer(0,0, t.terrainData.detailWidth, t.terrainData.detailHeight, 1);
		int startY = Mathf.RoundToInt (xymaxmin [3]);
		int endY = Mathf.RoundToInt (xymaxmin [2]);
		int startX = Mathf.RoundToInt (xymaxmin [1]);
		int endX = Mathf.RoundToInt (xymaxmin [0]);
		//Debug.Log(startY);
		//Debug.Log(endY);
		//Debug.Log(startX);
		//Debug.Log(endX);

		for (int y = startY; y < endY; y++) {
			for (int x = startX; x < endX; x++) {

				if (map [x, y] == 10) {
					count += 1;
					//Debug.Log ("Count:");
					//Debug.Log (count);
					float roughFloat = (count / baseRough) * 100;
					Mathf.Round (roughFloat);
					int roughInt = (int)roughFloat;
					roughText.text = "Rough Cut:" + roughInt + "%";

				}
				map[x,y] = 0;
				//Debug.Log(map[x,y]);
			}
		}
		t.terrainData.SetDetailLayer(0,0,1,map);
	}

	public void PlantGrass(Terrain t, Vector3 position, float radius)
	{


		int TerrainDetailMapSize = t.terrainData.detailResolution;
		if(t.terrainData.size.x != t.terrainData.size.z)
		{
			Debug.Log ("X and Y Size of terrain have to be the same (RemoveGrass.CS Line 43)");
			return;
		}

		float PrPxSize = TerrainDetailMapSize / t.terrainData.size.x;

		Vector3 TexturePoint3D = position - t.transform.position;
		TexturePoint3D = TexturePoint3D * PrPxSize;

		//Debug.Log(TexturePoint3D);

		float[] xymaxmin = new float[4];
		xymaxmin[0] = TexturePoint3D.z + radius;
		xymaxmin[1] = TexturePoint3D.z - radius;
		xymaxmin[2] = TexturePoint3D.x + radius;
		xymaxmin[3] = TexturePoint3D.x - radius;


		int[,] map = t.terrainData.GetDetailLayer(0,0, t.terrainData.detailWidth, t.terrainData.detailHeight, 0);

		int startY = Mathf.RoundToInt (xymaxmin [0]);
		Debug.Log(startY);
		for (int y = 0; y < t.terrainData.detailHeight; y++) {
			if(xymaxmin[2] > y && xymaxmin[3] < y)
			{
				for (int x = 0; x < t.terrainData.detailWidth; x++) {

					if(xymaxmin[0] > x && xymaxmin[1] < x)
						map[x,y] = 1;
				}
			}
		}
		t.terrainData.SetDetailLayer(0,0,0,map);
	}

	void CreateBackup(Terrain t)
	{
		Debug.Log ("DetailBackup Done");
		BackupMap = t.terrainData.GetDetailLayer(0,0, t.terrainData.detailWidth, t.terrainData.detailHeight, 0);
	}

	void OnDestroy() {
		Debug.Log ("I work !!! ");
		te.terrainData.SetDetailLayer(0,0,0, BackupMap);
	}

	int DetailMapCutoff(Terrain t, float threshold, int layer) {
		// Get all of layer zero.
		int[,] map = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, layer);
		int count = 0;
		// For each pixel in the detail map...
		for (int y = 0; y < t.terrainData.detailHeight; y++) {
			for (int x = 0; x < t.terrainData.detailWidth; x++) {
				// If the pixel value is below the threshold then
				// set it to zero.
				if (map[x, y] > threshold) {
					count += 1;
				}
			}
		}

		// Assign the modified map back.
		return count;

	}

}



	

