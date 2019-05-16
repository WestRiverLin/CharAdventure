using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.IO;
using System.Reflection;

public class TextDisplay : MonoBehaviour {
	public string fileName="story.txt";
	public string textStr = "文字测试\n";
	string[] textStrs;
	Dictionary<string,int> anchors;
	public float intervalTime=0.1f;
	float countTime=0f;
	int textIndex=-1;
	int textStrsIndex=-1;
	bool isTextEnd=false;
	bool isPause=false;
	public GameObject TextPanel;
	public Text DialogText;
	public GameObject DownArrow;
	public RawImage AvatarImage;
	public RawImage BackGroundImage;
	public GameObject OptionGroup;
	public GameObject OptionButton;
	

	public void Restart(){
		Application.LoadLevel(Application.loadedLevelName);
	}
	// Use this for initialization
	void Start () {
		InvokeMethod("TestTool.Print","DebugOutput",new string[]{"反射测试1","反射测试2","反射测试3"});
		textStrs=ReadFile(Path.Combine(Application.streamingAssetsPath,fileName));
		
		anchors=new Dictionary<string,int>();
		for(int i=0;i<textStrs.Length;i++){
			if(textStrs[i][0]=='#'){
				anchors.Add(textStrs[i].Split('#')[1],i);
			}
		}
		ClearText();
		RefreshImage();
		ReadLine();
	}

	void RefreshImage(){
		if(BackGroundImage.texture==null){
			BackGroundImage.color=new Color(0,0,0,1);
		}else{
			BackGroundImage.color=new Color(1,1,1,1);
		}
		if(AvatarImage.texture==null){
			AvatarImage.color=new Color(1,1,1,0);
		}else{
			AvatarImage.color=new Color(1,1,1,1);
		}
	}
	string GetTextStr(string str){
		Debug.Log("GetTextStr过滤字符串:"+str);
		if(str.Contains("@cmd")){
			return str.Split('@')[0];
		}
		return str;
	}
	public void JumpToAnchor(string anchor){
		Debug.Log("anchor:"+anchor);
		ClearText();
		ClearOptionGroup();
		isPause=false;
		textStrsIndex=anchors[anchor];
		ReadLine();
	}
	void ReplayStr(){
		ClearText();
		textIndex=0;
	}
	void SkipStr(){
		ClearText();
		textIndex=textStr.Length;
		DialogText.text=textStr;
	}
	void ClearText(){
		DialogText.text="";
	}
	void ClearOptionGroup(){
		int childCount=OptionGroup.transform.childCount;
		for(int i=0;i<childCount;i++){
			Destroy(OptionGroup.transform.GetChild(i).gameObject);
		}
		Debug.Log("清除选项:"+childCount);
	}
	void HideTextPanel(){
		Debug.Log("文本结束了");
		ClearText();
		TextPanel.SetActive(false);
	}
	void ShowTextPanel(){
		Debug.Log("文本开始了");
		TextPanel.SetActive(true);
		ReplayStr();
	}
	enum TextStrMode{
		None,
		Common,
		Option,
		Pause
	}
	TextStrMode AnalyseTextStr(){
		//True显示文本，False不显示文本
		textStr=textStr.Replace("\\n","\n");
		if(textStr[0]=='#'){
			//标签，直接跳过
			return TextStrMode.None;
			
		}else if(textStr.Contains("@cmd")){
			string[] args=textStr.Split('@');
			string cmd=args[1].Split(':')[1];
			Debug.Log("命令:"+cmd);
			switch(cmd){
				case "background":
				Debug.Log("执行命令："+cmd+" 参数:"+args[2]);
				if(args[2]=="none"){
					BackGroundImage.texture=null;
				}else{
					Texture tex=(Texture)Resources.Load(args[2])as Texture;
					BackGroundImage.texture=tex;
				}
				
				return TextStrMode.None;
				break;

				case "avatar":
				Debug.Log("执行命令："+cmd+" 参数:"+args[2]);
				if(args[2]=="none"){
					AvatarImage.texture=null;
				}else{
					Texture tex=(Texture)Resources.Load(args[2])as Texture;
					AvatarImage.texture=tex;
				}
				
				return TextStrMode.None;
				break;

				case "option":
				Debug.Log("执行命令："+cmd+" 参数:"+args[2]);
				isPause=true;
				for(int i=2;i<args.Length;i++){
					GameObject optionButton=Instantiate(OptionButton,new Vector3(0,0,0),Quaternion.Euler(0,0,0));
					optionButton.transform.parent=OptionGroup.transform;
					optionButton.GetComponent<RectTransform>().localPosition=new Vector2(0,100*(args.Length-3)/2-(i-1)*100);
					optionButton.transform.GetChild(0).GetComponent<Text>().text=args[i].Split(':')[0];
					string anchorStr=args[i].Split(':')[1];
					Debug.Log("AddListener:"+anchorStr);
					optionButton.GetComponent<Button>().onClick.AddListener(
						delegate(){
							JumpToAnchor(anchorStr);
						});
				}
				return TextStrMode.Option;
				break;

				case "pause":
				Debug.Log("执行命令："+cmd);
				isPause=true;
				return TextStrMode.Pause;
				break;

				default:
				Debug.Log("执行命令：default");
				return TextStrMode.Common;
				break;
			}
		}
		
		return TextStrMode.Common;
	}
	// Update is called once per frame
	void Update () {
		countTime+=Time.deltaTime;
		if(countTime>=intervalTime && textIndex<textStr.Length &&textIndex>=0){
			isTextEnd=false;
			countTime=0;
			DialogText.text+= textStr[textIndex];
			textIndex+=1;
		}
		if(textIndex>=textStr.Length){
			isTextEnd=true;
		}else{
			isTextEnd=false;
		}
		if(DownArrow.activeSelf!=isTextEnd){
			DownArrow.SetActive(isTextEnd);
		}
		//下一句
		if(
			Input.GetMouseButtonDown(0)//鼠标按下
		||
		(Input.touchCount==1&&Input.touches[0].phase==TouchPhase.Began)//手指按下
		){
			if(isTextEnd&&!isPause){

				ReadLine();

			}else{
				Debug.Log("快进");
				SkipStr();
			}
			
		}
	}

	void ReadLine(){

		if(textStrsIndex<textStrs.Length-1){
			Debug.Log("下一句,lineIndex="+textStrsIndex);

			//命令语句解析
			do{
				if(!(textStrsIndex<textStrs.Length-1)){
					HideTextPanel();
					break;
				}
				textStrsIndex+=1;
				textStr=textStrs[textStrsIndex];
				TextStrMode strMode=AnalyseTextStr();
				
				if(strMode==TextStrMode.None){
					//什么都不做，循环往下一行
				}else if(strMode==TextStrMode.Common){
					//break跳出循环
					break;
				}else if(strMode==TextStrMode.Option){
					//重新提取字符串
					textStr=GetTextStr(textStrs[textStrsIndex]);
					//break跳出循环
					break;
				}else if(strMode==TextStrMode.Pause){
					textStr=GetTextStr(textStrs[textStrsIndex]);
					break;
				}
			}while(true);

			RefreshImage();
			ReplayStr();
		}else{
			HideTextPanel();
		}

	}

	string[] ReadFile(string path) {
        return File.ReadAllLines(path);//读取文件的所有行，并将数据读取到定义好的字符数组strs中，一行存一个单元
    }

	/// 通过反射调用类的方法（SayHello(string name)）
        
	public void InvokeMethod(string ClassName,string MethodName,String[] Args)
	{
		// string nameSpace="StudyInvokeMethod";
		// string className="StudyInvokeMethod.HomeService";
		// string methodName="SayHello";
		// string param="李天";

		// string className="Print";
		// string methodName="DebugOutput";
		// string[] args = {"反射测试1","反射测试2","反射测试3"};
		string className=ClassName;
		string methodName=MethodName;
		string[] args = Args;
		
		Type t;
		t = Type.GetType(className);
		var obj = t.Assembly.CreateInstance(className);
		// MethodInfo[] info = t.GetMethods();
		// for (int i = 0; i < info.Length; i++)
        // {
        //     string str = info[i].Name;
        //     Debug.Log("方法名：" + str);
        // }
        MethodInfo method = t.GetMethod(methodName);
        //method.Invoke(obj, null);
		method.Invoke(obj, args);
	}

}
