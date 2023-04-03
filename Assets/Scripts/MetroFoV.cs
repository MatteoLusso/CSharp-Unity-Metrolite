using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetroFoV : MonoBehaviour
{
    public LineGenerator lineGen;
    public float speed;
    public float smooth;

    private AudioSource train_Noise;

    public Vector3[] coordinates;

    private bool started = false;

    void Start()
    {
        train_Noise = this.transform.GetComponent<AudioSource>();
        train_Noise.Stop();
        train_Noise.loop = true;
    }

    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(!started)
            {
                coordinates = lineGen.GetCurvePoints();

                if(coordinates != null)
                {
                    train_Noise.Play();
                    StartCoroutine("FollowRail");
                }
            }
            else
            {
                train_Noise.Stop();
                StopCoroutine("FollowRail");
            }
        }
    }

    IEnumerator FollowRail()
    {
        started = true;

        int i = 0;

        while((this.transform.position - new Vector3(coordinates[coordinates.Length - 1].x , coordinates[coordinates.Length - 1].y, coordinates[coordinates.Length - 1].z - 3.0f)).magnitude > 0.1f)
        {
            if((this.transform.position - new Vector3(coordinates[i].x , coordinates[i].y, coordinates[i].z - 3.0f)).magnitude > 0.1f)
            {
                //this.transform.position = Vector3.Lerp(this.gameObject.transform.position, new Vector3(coordinates[i].x , coordinates[i].y, coordinates[i].z - 3.0f), smooth * Time.deltaTime);
                this.transform.position += (new Vector3(coordinates[i].x , coordinates[i].y, coordinates[i].z - 3.0f) - this.transform.position).normalized * speed * Time.deltaTime;

                Debug.DrawLine(this.transform.position, new Vector3(coordinates[i].x , coordinates[i].y, coordinates[i].z - 3.0f), Color.yellow);
                Debug.Log((this.transform.position - new Vector3(coordinates[i].x , coordinates[i].y, coordinates[i].z - 3.0f)).magnitude);

                if(i < coordinates.Length - 1)
                {
                    this.transform.forward = Vector3.Lerp(this.transform.forward, coordinates[i + 1] - coordinates[i], smooth * Time.deltaTime);
                    this.transform.rotation *= Quaternion.Euler(0.0f, 0.0f, -90.0f);

                    Debug.DrawRay(this.transform.position, (coordinates[i + 1] - coordinates[i]).normalized * 10.0f, Color.green);
                    Debug.DrawRay(this.transform.position, this.transform.forward * 10.0f, Color.red);

                    //Debug.Log((i + " | " + coordinates[i] + " | " + coordinates[i + 1]));
                    //Debug.Log(Vector3.SignedAngle(this.gameObject.transform.forward, (coordinates[i + 1] - coordinates[i]).normalized, Vector3.up));
                }
            }
            else
            {
                i++;
            }
            yield return new WaitForEndOfFrame();
        }

        started = false;
    }
}
