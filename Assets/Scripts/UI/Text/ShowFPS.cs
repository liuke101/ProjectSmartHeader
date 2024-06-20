using TMPro;
using UnityEngine;

/// <summary>
/// 打印FPS
/// </summary>
public class ShowFPS : MonoBehaviour
{
    private TMP_Text _textMeshPro;
    
    public float _updateInterval = 1f;//设定更新帧率的时间间隔为1秒  
    private float _accum = .0f;//累积时间  
    private int _frames = 0;//在_updateInterval时间内运行了多少帧  
    private float _timeLeft;
    private string fpsFormat;

    private void Awake()
    {
        _textMeshPro = GetComponent<TMP_Text>();
    }
    
    void Start()
    {
        _timeLeft = _updateInterval;
    }
    //
    // void OnGUI()
    // {
    //     GUI.Label(new Rect(100, 100, 200, 200), fpsFormat);
    // }

    void Update()
    {
        _timeLeft -= Time.deltaTime;
        //Time.timeScale可以控制Update 和LateUpdate 的执行速度,  
        //Time.deltaTime是以秒计算，完成最后一帧的时间  
        //相除即可得到相应的一帧所用的时间  
        _accum += Time.timeScale / Time.deltaTime;
        ++_frames;//帧数  
        
        
        
        if (_timeLeft <= 0)
        {
            int fps = (int)(_accum / _frames);
            fpsFormat = $"帧率:{fps}";
            _textMeshPro.text = fpsFormat; //设置显示帧率的文本
            
            _timeLeft = _updateInterval;
            _accum = .0f;
            _frames = 0;
        }
    }
}
