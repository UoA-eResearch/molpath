using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Residue : MonoBehaviour
{

    public GameObject amide_pf;
    public GameObject calpha_pf;
    public GameObject carbonyl_pf;

    public float phiTarget;
    public float psiTarget;

    public float phiCurrent;
    public float psiCurrent;

    public GameObject ramaPlotOrigin;
    public Vector3 myPlotCubeBaseScale;
    public float ramaPlotScale = 0.0012f; //  set visually against UI

    public GameObject myPlotCube;
    float deltaScale = 1.0f;

    public bool residueSelected = false;
    public bool residueHovered = false;
    public bool residueGrabbed = false;

    public bool drivePhiPsiOn;
    public float drivePhiPsiTorqValue;

    void Start()
    {
        myPlotCube = Instantiate(myPlotCube);

        Transform menuContainer = GameObject.Find("UI_container").transform;
        if (menuContainer)
        {
            float menuContainerScale = menuContainer.localScale.x;
            ramaPlotScale *= menuContainerScale;
            myPlotCube.transform.localScale *= menuContainerScale;
        }

        ramaPlotOrigin = GameObject.Find("RamaPlotOrigin");
        myPlotCubeBaseScale = myPlotCube.transform.localScale;
        myPlotCube.transform.position = ramaPlotOrigin.transform.position;
        myPlotCube.transform.parent = gameObject.transform;
        myPlotCube.transform.rotation = ramaPlotOrigin.transform.rotation;
    }


    void MeasurePhiPsi()
    {
        phiCurrent = 180.0f - Vector3.SignedAngle(amide_pf.transform.up, calpha_pf.transform.up, amide_pf.transform.right);
        if (phiCurrent > 180.0f)
        {
            phiCurrent -= 360.0f;
        }
        //Debug.Log(phiTarget + " " + phiCurrent);

        psiCurrent = -Vector3.SignedAngle(calpha_pf.transform.up, carbonyl_pf.transform.up, calpha_pf.transform.right);

        //Debug.Log(psiTarget + " " + psiCurrent);
    }

    void UpdatePhiPsiPlotObj()
    {

        Renderer _myPlotCubeRenderer = myPlotCube.GetComponent<Renderer>();
        Vector3 deltaPos = new Vector3(0.0f, 0.0f, 0.0f);
        float targetDeltaScale = 1.0f;
        if (residueGrabbed)
        {
            _myPlotCubeRenderer.material.SetColor("_Color", Color.green);
            //deltaPos += ramaPlot.transform.forward * -0.015f;
            targetDeltaScale = 1.6f;
        }
        else
        {
            if (residueHovered)
            {
                _myPlotCubeRenderer.material.SetColor("_Color", Color.red);
                //deltaPos += ramaPlot.transform.forward * -0.01f;
                targetDeltaScale = 1.4f;
            }
            else
            {
                if (residueSelected)
                {
                    _myPlotCubeRenderer.material.SetColor("_Color", Color.yellow);
                    targetDeltaScale = 1.2f;
                }
                else
                {
                    _myPlotCubeRenderer.material.SetColor("_Color", Color.white);
                    targetDeltaScale = 1.0f;
                }
            }
        }


        myPlotCube.transform.rotation = ramaPlotOrigin.transform.rotation;
        myPlotCube.transform.position = ramaPlotOrigin.transform.position + (ramaPlotOrigin.transform.right * ramaPlotScale * phiCurrent) + (ramaPlotOrigin.transform.up * ramaPlotScale * psiCurrent) + deltaPos;
        deltaScale = Mathf.Lerp(deltaScale, targetDeltaScale, 0.2f);
        myPlotCube.transform.localScale = myPlotCubeBaseScale * deltaScale;
    }

    public bool IsResidueSelected()
    {
        BackboneUnit buAmide = amide_pf.GetComponent("BackboneUnit") as BackboneUnit;
        return buAmide.controllerSelectOn;
    }

    // Update is called once per frame
    void Update()
    {
        MeasurePhiPsi();
        UpdatePhiPsiPlotObj();
    }
}
