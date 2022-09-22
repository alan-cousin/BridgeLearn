using UnityEngine;

public class Player : MonoBehaviour
{
    public enum DIRECTION
    {
        LEFT,RIGHT,UP,DOWN
    };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Move(DIRECTION f_direct)
    {
        Debug.Log("MoveTo" + f_direct.ToString());
        Vector3 _curPos = transform.position;
        Debug.Log("Current Position = " + _curPos.ToString());
        _curPos.x = _curPos.x + (f_direct == DIRECTION.LEFT ? ConfigValues.CELL_SIZE_X : 0);
        _curPos.x = Mathf.Min(_curPos.x, ConfigValues.PANEL_SIZE_X);
        _curPos.x = _curPos.x + (f_direct == DIRECTION.RIGHT? -ConfigValues.CELL_SIZE_X : 0);
        _curPos.x = Mathf.Max(_curPos.x, 0);
        _curPos.z = _curPos.z + (f_direct == DIRECTION.UP? -ConfigValues.CELL_SIZE_Y : 0);
        _curPos.z = Mathf.Max(_curPos.z, 0);
        _curPos.z = _curPos.z + (f_direct == DIRECTION.DOWN? ConfigValues.CELL_SIZE_Y : 0);
        _curPos.z = Mathf.Min(_curPos.z, ConfigValues.PANEL_SIZE_Y);
        Debug.Log("Next Position = " + _curPos.ToString());
        int _posX =(int)( _curPos.x / (float)ConfigValues.CELL_SIZE_X);
        int _posY = (int)(_curPos.z / (float)ConfigValues.CELL_SIZE_Y);

        Debug.Log("Array Position = " + _posX + ":" + _posY);
        if (LevelManager.Instance.MovableTo(_posX, _posY)) {
            
            transform.position = LevelManager.Instance.MoveTo(_posX, _posY); ;
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        if( Input.GetKeyUp(KeyCode.A))
        {
            Move(DIRECTION.LEFT);
        }
        else if(Input.GetKeyUp(KeyCode.D))
        {
            Move(DIRECTION.RIGHT);
        }
        else if(Input.GetKeyUp(KeyCode.W))
        {
            Move(DIRECTION.UP);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            Move(DIRECTION.DOWN);
        }
        int player_appearance = ConfigValues.PLAYER_APPEARANCE[LevelManager.Instance.Level];
        Vector3 cur_scale = transform.GetChild(0).transform.localScale;
        transform.GetChild(0).transform.localScale = new Vector3(player_appearance, cur_scale.y, player_appearance);
    }
}
