using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class grassCutter : MonoBehaviour {
	public float Radius;
	public LayerMask layerMask;
	public terrainControl tc;

	protected int[,] BackupMap;
	public Terrain te;
	public GameObject mower;
	Vector3 pos;
	bool mowerOn = false;

	public Text fairwayText;
	private float count;
	private float baseFairway = 0f;

	// Use this for initialization
	void Start () {

		CreateBackup(te);
		count = 0f;
		baseFairway = (float)DetailMapCutoff (te, 0, 0);
		fairwayText.text = "Fairway Cut: 0%";


		Debug.Log (baseFairway);
	}

	void Update()
	{

		terrainControl.terrainArea ta;
		int[,] test;
		ta = tc.terrainAreaFromWorld (mower.transform.position.x, mower.transform.position.z, 8.0f, 8.0f);
		test = tc.getDetailLayer (ta, 1);
		if (Input.GetKeyDown ("s")) {
			mowerOn = !mowerOn;
		}


		//Debug.Log (test);

		//NOTE FOR TESTING
		/*
  if(Input.GetMouseButton(0))
  {
   RaycastHit hit;
   Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
   if(Physics.Raycast(ray,out hit , 99999999f, layerMask))
   {
    Debug.Log (hit.point);
    CutGrass(null, hit.point, Radius);
   }
  }
  
  if(Input.GetMouseButton(1))
  {
   RaycastHit hit;
   Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
   if(Physics.Raycast(ray,out hit , 99999999f, layerMask))
   {
    Debug.Log (hit.point);
    PlantGrass(null, hit.point, Radius);
   }
  }
  */
		pos = mower.transform.position;

		if (mowerOn == true) {
			CutGrass (te, pos, 4.0f);
		}
	}

	///
	/// Cuts the grass. BE CAREFULL: Grass may not respawn after game reset!!!!!! 
	///
	/// The effected Terrain, if only one Terrain pass null
	/// The world position you want to cut the grass
	/// the radius of the square
	public void CutGrass(Terrain t, Vector3 position, float radius)
	{


		int TerrainDetailMapSize = t.terrainData.detailResolution;
		/* if(t.terrainData.size.x != t.terrainData.size.z)
		{
			Debug.Log ("X and Y Size of terrain have to be the same (RemoveGrass.CS Line 43)");
			return;
		} */

		float PrPxSize = TerrainDetailMapSize / t.terrainData.size.x;

		Vector3 TexturePoint3D = position - t.transform.position;
		TexturePoint3D = TexturePoint3D * PrPxSize;

		Debug.Log(TexturePoint3D);

		float[] xymaxmin = new float[4];
		xymaxmin[0] = TexturePoint3D.z + radius;
		xymaxmin[1] = TexturePoint3D.z - radius;
		xymaxmin[2] = TexturePoint3D.x + radius;
		xymaxmin[3] = TexturePoint3D.x - radius;


		int[,] map = t.terrainData.GetDetailLayer(0,0, t.terrainData.detailWidth, t.terrainData.detailHeight, 0);
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
				//Debug.Log(startY);
				//Debug.Log(endY);
				//if(xymaxmin[0] > x && xymaxmin[1] < x && xymaxmin[2] > y && xymaxmin[3] < y)
				if (map [x, y] == 10) {
					count += 1;
					//Debug.Log ("Count:");
					//Debug.Log (count);
					float fairwayFloat = (count / baseFairway) * 100;
					Mathf.Round (fairwayFloat);
					int fairwayInt = (int)fairwayFloat;
					fairwayText.text = "Fairway Cut:" + fairwayInt + "%";
				}

				map [x, y] = 0;
				//Debug.Log(map[x,y]);
			}
		}
		t.terrainData.SetDetailLayer(0,0,0,map);
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

		Debug.Log(TexturePoint3D);

		float[] xymaxmin = new float[4];
		xymaxmin[0] = TexturePoint3D.z + radius;
		xymaxmin[1] = TexturePoint3D.z - radius;
		xymaxmin[2] = TexturePoint3D.x + radius;
		xymaxmin[3] = TexturePoint3D.x - radius;


		int[,] map = t.terrainData.GetDetailLayer(0,0, t.terrainData.detailWidth, t.terrainData.detailHeight, 0);

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

