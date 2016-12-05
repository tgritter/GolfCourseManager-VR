/********************************************
 * Unity 3d Runtime Terrain Tools, by Sam Serrels ("@TheDooglz", sam@nanochimps.com)
 * This code was published for purchase on the Unity3d Asset store, and should not be redistributed anywhere else
 * http://unity3d.com/company/legal/as_terms -- http://unity3d.com/company/legal/as_provider
 ***********************************************/

using UnityEngine;
using System.Collections;
/**
 * This is main class, which contains all the main functions.
 * It doesn't have to be attached to a terrain object, 
 * just make sure to either set the "ter" variable in the editor, or use the setTerrain function.
 */
public class terrainControl : MonoBehaviour {
	private int verboseLevel;
	public Terrain ter; //!< The terrain Instance to use
	private TerrainData terdata;
	public int randomSeed;
	/// A generic struct to hold a Terrain segments.
	public struct terrainArea
	{
		public int x, z, sizeX, sizeZ, x2,z2;
		//array keeps synced to the size fo the tA, but does not care for it's contents
		private float[,] array;
		public terrainArea(int X, int Z, int Xsize, int Zsize)
		{
			x = X;
			z = Z;
			sizeX = Xsize;
			sizeZ = Zsize;
			x2 = x + sizeX;
			z2 = z + sizeZ;
			array = new float[Xsize,Zsize];
		}
		public float[,] getArray(){
			if (array == null || array.GetLength(0) != sizeX+1 || array.GetLength(1) != sizeZ+1){
				array = new float[sizeX+1,sizeZ+1];
			}
			return array;
		}
	}
	void Awake() {
		verboseLevel = 2;
		if(ter == null){
			echo(1,"Terrain is not set (now searching for any terrain to use)!");
			ter = Terrain.activeTerrain;
			if(ter == null){
				echo(1,"ERROR: No terrain found!");
			}
		}
		terdata = ter.terrainData;
	}
	void Start () {

	}

	void Update () {

	}
	/// randomly deforms a terrain area, using perlin noise.
	public void randomizeArea(float minheight, float maxheight,terrainArea area,float smoothness){
		echo(3,"randomizeArea() called ");
		smoothness = (0.9888f/smoothness);
		float[,] arr = area.getArray();
		for(int i = 0; i<= area.sizeX ;i++){
			for(int j = 0; j<= area.sizeZ ;j++){
				arr[i,j] = ((maxheight-minheight)*(Mathf.PerlinNoise((area.x+i+randomSeed)*smoothness,(area.z+j+randomSeed)*smoothness)))+minheight;
			}
		}	
		terdata.SetHeights(area.x, area.z, arr);	
	}
	/// Set the detail data of a TerrainArea, see the Alphamaps and Details page for more info.
	public void setAreaDetails(terrainArea area,int layer,int[,] detailMix){
		echo(3,"setAreaDetails() called ");
		terdata.SetDetailLayer(area.x,area.z,layer,detailMix);
	}
	/// Set the texture data of a TerrainArea according to an array, see the Alphamaps and Details page for more info.
	public void setAreaTextureByHeight(terrainArea area,float[,] textureMix){
		echo(3,"setAreaTextureByHeight called ");
		float scaler = (terdata.alphamapResolution/(terdata.heightmapResolution-1));
		float[,,] arr = terdata.GetAlphamaps((int)(area.x*scaler),(int)(area.z*scaler),(int)(area.sizeX*scaler),(int)(area.sizeZ*scaler));
		int layers = arr.GetLength(2);
		float scalerX = terdata.size.x/terdata.alphamapResolution;
		float scalerZ = terdata.size.z/terdata.alphamapResolution;
		for ( int i = 0; i < arr.GetLength(0);i++ ) {
			for ( int j = 0; j < arr.GetLength(1);j++ ) {
				//float height = terdata.GetHeight(Mathf.RoundToInt(area.x+(j/scaler)),Mathf.RoundToInt(area.z+(i/scaler)));
				float height  = ter.SampleHeight(new Vector3(j*scalerX,0,i*scalerZ));
				bool found = false;
				for ( int k = 0; k < (textureMix.GetLength(0)-1);k++ ) {
					if(height >= textureMix[k,0] && height < textureMix[k+1,0]){
						found = true;
						float balence = Mathf.InverseLerp(textureMix[k,0], textureMix[k+1,0], height);
						for ( int m = 0; m < layers; m+=1 ){
							if(m==textureMix[k,1]&&textureMix[k,1]==textureMix[k+1,1]){
								arr[i,j,m] = 1.0f;
							}else if(m == Mathf.RoundToInt(textureMix[k,1])){
								arr[i,j,m] = 1.0f-balence;
							}else if(m == Mathf.RoundToInt(textureMix[k+1,1])){
								arr[i,j,m] = balence;
							}else{
								arr[i,j,m] = 0.0f;
							}
						}
						break;
					}
				}
				if(!found){
					int tex = (int)textureMix[textureMix.GetLength(0)-1,1];
					if(tex >= layers){tex = layers-1;}
					for ( int m = 0; m < layers; m+=1 ){
						if(m == tex){
							arr[i,j,m] = 1.0f;
						}else{
							arr[i,j,m] = 0.0f;
						}
					}
				}
			}
		}
		terdata.SetAlphamaps((int)(area.x*scaler),(int)(area.z*scaler),arr);
	}
	/// Set the texture data of a TerrainArea, see the Alphamaps and Details page for more info.
	public void setAreaTexture(terrainArea area,float[] textureMix){
		echo(3,"setAreaTexture() called ");
		float scaler = (terdata.alphamapResolution/(terdata.heightmapResolution-1));
		float[,,] arr = terdata.GetAlphamaps((int)(area.x*scaler),(int)(area.z*scaler),(int)(area.sizeX*scaler),(int)(area.sizeZ*scaler));
		//validate mix
		int layers = arr.GetLength(2);
		if(textureMix.Length > layers){
			echo(2,"setAreaTexture(), Supplied textureMix has: "+textureMix.Length+" layers, the terrain only has: "+layers+", program will disregard the extra layers");
		}
		float total = 0;
		for ( int i = 0; i < layers;i+=1 ) {
			total += textureMix[i];
		}
		if(total != 1.0f){
			echo(1,"setAreaTexture(), textureMix results in: "+total+" it should add up to 1.0f! ");
		}else{
			int a =0;
			echo(3,"Good textureMix!");
			for ( int i = 0; i < arr.GetLength(0);i++ ) {
				for ( int j = 0; j < arr.GetLength(1);j++ ) {
					for ( int k = 0; k < layers; k+=1 ){
						arr[i,j,k] = textureMix[k];
						a++;
					}
				}
			}
			echo(3,"a:"+a);
			terdata.SetAlphamaps((int)(area.x*scaler),(int)(area.z*scaler),arr);
		}
	}
	/// Get an array of detail data from a TerrainArea
	public int[,] getDetailLayer(terrainArea area, int layer){
		echo(3,"GetDetailLayer(TerrainArea) called ");
		return terdata.GetDetailLayer(area.x,area.z,area.sizeX,area.sizeZ,layer);
	}
	/// Get an array of texture data from a TerrainArea
	public float[,,] getAlphamaps(terrainArea area){
		echo(3,"getAlphamaps(TerrainArea) called ");
		return terdata.GetAlphamaps(area.x,area.z,area.sizeX,area.sizeZ);
	}
	/** Returns the average height in a given area.
	 * A higher speed value gives less accurate results,
	 * Set speed to 1 to sample every point, 2 to sample 50% of the points, 3 for 30% etc...
	 * For large areas use a higher number for speed to avoid slowdowns.
	 */
	public float averageHeight(terrainArea area, int speed){
		if(speed <= 0){
			speed = 1;
		}
		if(speed > (int)(area.sizeX/2)){
			speed = (int)(area.sizeX/2);
		}
		if(speed > (int)(area.sizeZ/2)){
			speed = (int)(area.sizeZ/2);
		}
		float[,] arr = getHeights(area);
		float total =0;
		float itt=0;
		for ( int i = 0; i < area.sizeX;i+=speed ) {
			for ( int j = 0; j < area.sizeZ;j+=speed ) {
				if(i <= arr.GetLength(0) && j <= arr.GetLength(1)){
					total += (float)arr[i,j];
					itt++;
				}
			}
		}
		echo(3," averageHeight(terrainArea) called, Speed: "+speed+" result: "+(total/itt));
		return (total/itt);
	}
	/** sets the height of a terrain area according to an array of heights.
	 *  clip:true, only set the heights within a terrainArea, even if the array is bigger<br>
	 *	clip:false, if the supplied array is bigger than the texture area, change heights outside of the area also
	 */
	public void setHeights(terrainArea area, float[,] heights, bool clip){
		echo(3,"SetHeights(terrainArea) called ");
		if(clip && (heights.GetLength(0) > area.sizeX || heights.GetLength(1) > area.sizeZ)){
			Transmute2d(area.getArray(),heights);
			terdata.SetHeights(area.x, area.z, area.getArray());	
		}else{
			terdata.SetHeights(area.x, area.z, heights);	
		}
	}
	/// Get an array of heightmap samples from a TerrainArea
	public float[,] getHeights(terrainArea area){
		echo(3,"getHeights(TerrainArea) called ");
		if(!validateArea(area)){
			return null;
		}
		return terdata.GetHeights(area.x, area.z, area.sizeX, area.sizeZ);
	}
	/** sets the height of a terrain area.
	 *  So long as (0.0f <= height >= 1.0f)
	 */
	public void setHeight(terrainArea area, float height){
		echo(3,"setHeight(terrainArea) called ");
		Populate2d(area.getArray(),height);
		terdata.SetHeights(area.x, area.z, area.getArray());
	}
	///sets the height of a terrain area to a global world(Y-axis) height.
	public void setHeightW(terrainArea area, float worldHeight){
		echo(3,"setHeightW(terrainArea) called ");
		Populate2d(area.getArray(),worldToTerrainSpace(new Vector3(0,worldHeight,0)).y);
		terdata.SetHeights(area.x, area.z, area.getArray());
	}
	/// Generates a terrainArea from worldspace coordiantes.
	public terrainArea terrainAreaFromWorld(float worldX, float worldZ, float sizeX, float sizeZ){
		Vector3 pos = worldToTerrainSpace(new Vector3(worldX,0,worldZ));
		int Xsize = (int)(sizeX*(terdata.heightmapResolution/terdata.size.x));
		int Zsize = (int)(sizeZ*(terdata.heightmapResolution/terdata.size.z));
		echo(3,"getTerrainAreaFromWorld: "+(int)pos.x+" , "+(int)pos.z+" , "+Xsize+" , "+Zsize+"");
		terrainArea ta = new terrainArea((int)pos.x,(int)pos.z,Xsize,Zsize);
		validateArea(ta);
		return ta;
	}
	/** sets the height of every point in the terrain.
	 *  So long as (0.0f <= height >= 1.0f)
	 */
	public void flatten(float height){
		int res = terdata.heightmapResolution;
		float[,] heights = new float[res,res];
		Populate2d(heights,height);
		echo(3,"flattening the terrain to height: "+height);
		terdata.SetHeights(0, 0, heights);	
	}
	/// sets the height of every point in the terrain, to the global world(Y-axis) height.
	public void flattenW(float worldHeight){
		echo(3,"flattening the terrain to worldHeight: "+worldHeight);
		flatten(worldToTerrainSpace(new Vector3(0,worldHeight,0)).y);
	}
	///Returns the given World position in terrainspace.
	public Vector3 worldToTerrainSpace(Vector3 worldPosition){
		int posx = (int)((worldPosition.x-ter.transform.position.x)/(terdata.size.x/terdata.heightmapResolution));
		int posz = (int)((worldPosition.z-ter.transform.position.z)/(terdata.size.z/terdata.heightmapResolution));
		float posy = Mathf.Clamp((worldPosition.y-ter.transform.position.y)/(terdata.size.y/1),0.0f,1.0f);
		echo(3,"WorldToTerrain: "+worldPosition+" World->Terrain "+new Vector3(posx,posy,posz));
		return new Vector3(posx,posy,posz);
	}
	/**Returns the given Terrain position in worldspace.
	 *  due to rounding, using this function to reverse a worldToTerrainSpace(), will give different data to the original input position.
	 */
	public Vector3 terrainToWorldSpace(Vector3 terrainposition){
		//expect innacuracies here.
		float posx = ((terrainposition.x*(terdata.size.x/(terdata.heightmapResolution-1)))+ter.transform.position.x);
		float posz = ((terrainposition.z*(terdata.size.z/(terdata.heightmapResolution-1)))+ter.transform.position.z);
		float posy = ((terrainposition.y*terdata.size.y)-ter.transform.position.y);
		echo(3,"terrainToWorldSpace: "+terrainposition+" terrain->world "+new Vector3(posx,posy,posz));
		return new Vector3(posx,posy,posz);

	}
	///Validates a TerrainArea, making sure it is whithin the bounds of the terrain.
	public bool validateArea(terrainArea ta){
		bool res = true;
		string str = "Terrain area out of bounds!";
		if(ta.x > (int)terdata.heightmapResolution){
			res = false;
			str += ", X coOrd "+ta.x+" is to large, must be <"+(int)terdata.heightmapResolution;
		}else if((ta.x+ta.sizeX)> (int)terdata.heightmapResolution){
			res = false;
			str += ", X dimension size to large";
		}
		if(ta.z > (int)terdata.heightmapResolution){
			res = false;
			str += ", Z coOrd ("+ta.z+") is to large, must be <"+(int)terdata.heightmapResolution;
		}else if((ta.z+ta.sizeZ)> (int)terdata.heightmapResolution){
			res = false;
			str += ", Z dimension size to large";
		}
		if(!res){
			echo(1,str);
		}
		return res;
	}
	///Sets the Terrain to use.
	public void setTerrain(Terrain terrain){
		ter = terrain;
	}
	///returns the currently set Terrain.
	public Terrain getTerrain(){
		return ter;
	}
	///returns the TerainData object of the current terrain.
	public TerrainData getTerrainData(){
		return terdata;
	}
	/// @name Debug/helper functions
	///@{
	private void echo(int lvl, string txt){
		if(verboseLevel > lvl){
			if(lvl <= 1){
				Debug.LogError("TRRN: "+txt);
			}else if(lvl == 2){
				Debug.LogWarning("TRRN: "+txt);
			}else{
				Debug.Log("TRRN: "+txt);
			}
		}
	}
	///prints interesting stuff about the terrain.
	public void getTerrainInfo(){
		Debug.Log("Interesting stuff about the terrain");
	}
	///fills a 2d Array with the given value.
	public void Populate2d<T>(T[,] arr, T value ){
		for ( int i = 0; i < arr.GetLength(0);i++ ) {
			for ( int j = 0; j < arr.GetLength(1);j++ ) {
				arr[i,j] = value;
			}
		}
	}
	///copies everything from sourceArr to targetArr, if sourceArr is bigger, the excess data is disregarded. 
	public void Transmute2d<T>(T[,] targetArr, T[,] sourceArr){
		for ( int i = 0; i < targetArr.GetLength(0);i++ ) {
			for ( int j = 0; j < targetArr.GetLength(1);j++ ) {
				if(i <= sourceArr.GetLength(0) && j <= sourceArr.GetLength(1)){
					targetArr[i,j] = sourceArr[i,j];
				}
			}
		}
	}
	/** Set's the amount of console output the code generates.
	 *  0 = no output, 1 =  errors, 2 = errors and warnings (default), 3 =  Loads of debug info
	 */
	public void setVerboseLevel(int lvl){
		verboseLevel = Mathf.Clamp(lvl,0,6);
	}
	///@}
}

