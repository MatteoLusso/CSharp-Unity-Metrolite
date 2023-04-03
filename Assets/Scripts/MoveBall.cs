using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBall : MonoBehaviour
{

    public float speed;
    public float rot;
    public float smooth_Axis;
    public float smooth_Button;

    private float input_Vertical1;
    private float input_Horizontal1;
    private float input_Vertical2;
    private float input_Horizontal2;
    private float input_LB;
    private float input_RB;

    void Start()
    {
        input_Vertical1 = 0.0f;
        input_Horizontal1 = 0.0f;
        input_Vertical2 = 0.0f;
        input_Horizontal2 = 0.0f;
        input_LB = 0.0f;
        input_RB = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(Mathf.Abs(Input.GetAxis("Vertical")) >= 0.15f)
        {
            input_Vertical1 = Mathf.Lerp(input_Vertical1, Input.GetAxis("Vertical"), smooth_Axis * Time.deltaTime);
        }
        else
        {
            input_Vertical1 = Mathf.Lerp(input_Vertical1, 0.0f, smooth_Axis * Time.deltaTime);
        }

        if(Mathf.Abs(Input.GetAxis("Horizontal")) >= 0.15f)
        {
            input_Horizontal1 = Mathf.Lerp(input_Horizontal1, Input.GetAxis("Horizontal"), smooth_Axis * Time.deltaTime);
        }
        else
        {
            input_Horizontal1 = Mathf.Lerp(input_Horizontal1, 0.0f, smooth_Axis * Time.deltaTime);
        }

        if(Mathf.Abs(Input.GetAxis("Vertical2")) >= 0.3f)
        {
            input_Vertical2 = Mathf.Lerp(input_Vertical2, Input.GetAxis("Vertical2"), smooth_Axis * Time.deltaTime);
        }
        else
        {
            input_Vertical2 = Mathf.Lerp(input_Vertical2, 0.0f, smooth_Axis * Time.deltaTime);
        }

        if(Mathf.Abs(Input.GetAxis("Horizontal2")) >= 0.8f)
        {
            input_Horizontal2 = Mathf.Lerp(input_Horizontal2, Input.GetAxis("Horizontal2"), smooth_Axis * Time.deltaTime);
        }
        else
        {
            input_Horizontal2 = Mathf.Lerp(input_Horizontal2, 0.0f, smooth_Axis * Time.deltaTime);
        }

        if(Input.GetButton("LB"))
        {
            input_LB = Mathf.Lerp(input_LB, 1.0f, smooth_Button * Time.deltaTime);
        }
        else
        {
            input_LB = Mathf.Lerp(input_LB, 0.0f, smooth_Button * Time.deltaTime);
        }

        if(Input.GetButton("RB"))
        {
            input_RB = Mathf.Lerp(input_RB, 1.0f, smooth_Button * Time.deltaTime);
        }
        else
        {
            input_RB = Mathf.Lerp(input_RB, 0.0f, smooth_Button * Time.deltaTime);
        }

        //----//

        this.transform.position += this.transform.forward * speed * input_Vertical1 * Time.deltaTime;
        //this.transform.position += this.transform.right * speed * input_Horizontal1 * Time.deltaTime;

        /*this.transform.up = Quaternion.AngleAxis(-rot * input_RB * Time.deltaTime, this.transform.forward) * this.transform.up;
        this.transform.up = Quaternion.AngleAxis(rot * input_LB * Time.deltaTime, this.transform.forward) * this.transform.up;*/

        //this.transform.forward = Quaternion.AngleAxis(rot * input_Vertical2 * Time.deltaTime, this.transform.right.normalized) * this.transform.forward;
        //this.transform.forward = Quaternion.AngleAxis(rot * input_Horizontal2 * Time.deltaTime, this.transform.up) * this.transform.forward;

        //this.transform.eulerAngles += new Vector3(-rot * input_Vertical2, rot * input_Horizontal2, ((rot * input_LB) - (rot * input_RB))) * Time.deltaTime;

        this.transform.Rotate(new Vector3(-input_Vertical2, -(input_LB - input_RB), -input_Horizontal2) * rot * Time.deltaTime);

        //this.transform.forward = Quaternion.AngleAxis(speed * Input.GetAxis("Vertical2"))
    }
}
