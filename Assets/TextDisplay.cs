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
	public Text CurrentText;
	public GameObject TextPanel;
	public RawImage CurrentImage;
	public float intervalTime=0.1f;
	float countTime=0f;
	int textIndex=0;
	int textStrsIndex=0;
	public GameObject DownArrow;
	bool isTextEnd=false;
	// Use this for initialization
	void Start () {
		InvokeMethod("TestTool.Print","DebugOutput",new string[]{"反射测试1","反射测试2","反射测试3"});
		textStrs=ReadFile(Path.Combine(Application.streamingAssetsPath,fileName));
		textStr=textStrs[textStrsIndex];
		ClearText();
		AnalyseTextStr();
	}

	void RefreshImage(){
		if(CurrentImage.texture==null){
			CurrentImage.color=new Color(1,1,1,0);
		}else{
			CurrentImage.color=new Color(1,1,1,1);
		}
	}
	void ReplayStr(){
		ClearText();
		textIndex=0;
	}
	void SkipStr(){
		ClearText();
		textIndex=textStr.Length;
		CurrentText.text=textStr;
	}
	void ClearText(){
		CurrentText.text="";
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
	bool AnalyseTextStr(){
		textStr=textStr.Replace("\\n","\n");
		if(textStr.Contains("cmd")){
			string cmd=textStr.Split('@')[1].Split(':')[1];
			string param=textStr.Split('@')[2];
			Debug.Log("命令:"+cmd+" 参数:"+param);
			if(cmd=="image"){
				Debug.Log("执行命令："+cmd);
				Texture tex=(Texture)Resources.Load(param)as Texture;
				CurrentImage.texture=tex;
			}
			//命令文本
			RefreshImage();
			return false;
		}
		//可显示文本
		RefreshImage();
		return true;
	}
	// Update is called once per frame
	void Update () {
		
		countTime+=Time.deltaTime;
		if(countTime>=intervalTime && textIndex<textStr.Length){
			isTextEnd=false;
			countTime=0;
			CurrentText.text+= textStr[textIndex];
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
		if(Input.GetMouseButtonDown(0)){
			if(isTextEnd){

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
						
					}while(!AnalyseTextStr());

					ReplayStr();
				}else{
					HideTextPanel();
				}

			}else{
				Debug.Log("快进");
				SkipStr();
			}
			
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
