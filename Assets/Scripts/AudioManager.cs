using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {
    

    public AudioMixer MasterMixer;
	public AudioMixer MusicMixer;
	public AudioMixer SFXMixer;
    AudioMixerSnapshot MusicZoomedIn;
    AudioMixerSnapshot MusicZoomedOut;
    AudioMixerSnapshot MusicBlockZilla;

	AudioMixerSnapshot SFXZoomedIn;
	AudioMixerSnapshot SFXZoomedOut;

    float[] Weights = new float[2] { 0, 1 };
    public float TransitionSpeed = 0.2f;
    AudioMixerSnapshot[] musicZoomSnapshots;
	AudioMixerSnapshot[] sfxZoomSnapshots;

	AudioSource[] music;
    bool EventActive;
    void OnEnable()
    {
		EventHandler.BlockzillaSpawned += TransitionToAttackMusic;
		EventHandler.BlockzillaDead += TransitionToNormalMusic;
        //CameraControls.ZoomedIn += TransitionToZoomedIn;
       // CameraControls.ZoomedOut += TransitionToZoomedOut;
    }

    void OnDisable()
    {
		EventHandler.BlockzillaSpawned -= TransitionToAttackMusic;
		EventHandler.BlockzillaDead -= TransitionToNormalMusic;

        //CameraControls.ZoomedIn -= TransitionToZoomedIn;
        //CameraControls.ZoomedOut -= TransitionToZoomedOut;
    }

    // Use this for initialization
    void Start () {

		music = Camera.main.GetComponents<AudioSource> ();
        MusicZoomedOut = MusicMixer.FindSnapshot("ZoomedOut");
        MusicZoomedIn = MusicMixer.FindSnapshot("ZoomedIn");

		SFXZoomedOut = SFXMixer.FindSnapshot("ZoomedOut");
		SFXZoomedIn = SFXMixer.FindSnapshot("ZoomedIn");

        MusicBlockZilla = MusicMixer.FindSnapshot("BlockZillaAttack");


        musicZoomSnapshots = new AudioMixerSnapshot[2] { MusicZoomedIn, MusicZoomedOut };
		sfxZoomSnapshots = new AudioMixerSnapshot[2] { SFXZoomedIn, SFXZoomedOut };

    }
	
	// Update is called once per frame
	void Update () {
		//SFXMixer.TransitionToSnapshots(sfxZoomSnapshots, Weights, TransitionSpeed);
	}

    void TransitionToZoomedIn()
    {
        if (EventActive)
        {
            return;
        }
        MusicZoomedIn.TransitionTo(TransitionSpeed);
		SFXZoomedIn.TransitionTo (TransitionSpeed);
    }

    void TransitionToZoomedOut()
    {
        if (EventActive)
        {
            return;
        }
        MusicZoomedOut.TransitionTo(TransitionSpeed);
		SFXZoomedOut.TransitionTo(TransitionSpeed);

    }

    void TransitionZoomMusic()
    {
        //print("Activated");
		MusicMixer.TransitionToSnapshots(musicZoomSnapshots, Weights, TransitionSpeed);
    }

    void TransitionToAttackMusic()
    {
		if (music [2].isPlaying) {
			
			return;
		}

		EventActive = true;
		music [2].Play ();
		MusicBlockZilla.TransitionTo(TransitionSpeed);
    }

    void TransitionToNormalMusic()
    {
        EventActive = false;
        ZoomLevelMusicBlend();
		StartCoroutine (WaitMusicStop ());
    }

	IEnumerator WaitMusicStop()
	{
		yield return new WaitForSeconds (1.5f);
		music [2].Stop ();

	}

    void ZoomLevelMusicBlend()
    {
		MusicMixer.TransitionToSnapshots(musicZoomSnapshots, Weights, TransitionSpeed);
    }

    public void SetZoomLevelWeights(float[] weightarray)
    {
        Weights[0] = weightarray[0];
        Weights[1] = weightarray[1];
    }

}
