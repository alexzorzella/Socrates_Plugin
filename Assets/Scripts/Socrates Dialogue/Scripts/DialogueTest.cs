using System;
using PlasticGui;
using SocratesDialogue;
using UnityEngine;

public class DialogueTest : MonoBehaviour {
    public Animator anim;
    public Transform speakingIcon;
    public BoxCollider2D disableCollider;

    bool done;

    MultiAudioSource ringUpDing;

    public int cents = 10;

    void Start() {
        ringUpDing = MultiAudioSource.FromResource(gameObject, "ring_up_ding");
    }

    public DialogueSection Dialogue() {
        DialogueBuilder builder = new DialogueBuilder();

        builder.WithSection(
            new SectionBuilder(
                    Namepedia.Clerk,
                    "Good evening. What can I do for you?",
                    "a").
                WithChoice("A six pack, please", "b").
                WithChoice("About the jar", "explain").
                WithChoice("About the exit", "exit").
                WithChoice("I'll be back", ""));
        
        builder.WithSection(new SectionBuilder(Namepedia.Clerk, "That'll be $5.94.", "b").
            WithNextSection(cents >= 594 ? "has" : "doesNotHave"));
        
        builder.WithSequentialSections(
            new SectionBuilder(Namepedia.Mummy, $"Here, I'm buying for Niall.", "has"),
            new SectionBuilder(Namepedia.Clerk, "One second... I've radioed for a monocopter, get to the pad. Don't forget the six pack!").
                WithAction(() => {
                    // anim.SetTrigger("ring_up");
                    // GameManager.Instance().stats.cents = Mathf.Clamp(GameManager.Instance().stats.cents - 594, 0, int.MaxValue);
                    // ringUpDing.Play();
                    // done = true;
                    Debug.Log("anim..., GameManager..., ringUpDing..., done...");
                })
            );
        
        builder.WithSequentialSections(
           new SectionBuilder(Namepedia.Mummy, $"I, uh, must have dropped my wallet. Can I get it for {cents.ToString()}?", "doesNotHave"),
           new SectionBuilder(Namepedia.Clerk, "No money, no beer. I would ID you, but you look decrepit, man."),
           new SectionBuilder(Namepedia.Mummy, "Pharaohs get no street cred these days."),
           new SectionBuilder(Namepedia.Clerk, "You'll have street cred when you get some pockets for your wallet. Now scram, I'm busy.")
           );
        
        builder.WithSequentialSections(
            new SectionBuilder(Namepedia.Clerk, "That? Well, let's first get something straight.", "explain"),
            new SectionBuilder(Namepedia.Clerk, "You've got Egyptians building pyramids in Egypt, right?"),
            new SectionBuilder(Namepedia.Clerk, "Well, this is dimensional debris."),
            new SectionBuilder(Namepedia.Clerk, "Nobody comes here, to Antarctica. The only people here are work professionals."),
            new SectionBuilder(Namepedia.Clerk, "What did scientists do? They use this as a testing ground."),
            new SectionBuilder(Namepedia.Clerk, "The ones in this dimension did a lot of time travel testing, stuff to do with other Egypts in parallel dimensions and so forth."),
            new SectionBuilder(Namepedia.Clerk, "Out here there are tons of old tombs and such."),
            new SectionBuilder(Namepedia.Clerk, "I hauled this jar here from the valley west of here."),
            new SectionBuilder(Namepedia.Clerk, "I bet it's worth millions, but customs would snatch it away and dump it in some wasted dimension.")
        );
        
        builder.WithSequentialSections(
            new SectionBuilder(Namepedia.Clerk, "Oh, is the driveway all snowed up?", "exit"),
            new SectionBuilder(Namepedia.Clerk, $"{Namepedia.Woman}'s on the other shift, she usually shovels the snow."),
            new SectionBuilder(Namepedia.Mummy, "No, it's hard, like ice hard."),
            new SectionBuilder(Namepedia.Clerk, "Eh, the crew has ice picks. They come over every so often. I've got beer and TV, I'm set."),
            new SectionBuilder(Namepedia.Mummy, "Look, I need to get out of here either way, come on."),
            new SectionBuilder(Namepedia.Clerk, "All right, you know what? You can have my explosives."),
            new SectionBuilder(Namepedia.Clerk, "Aim in the direction you want to place them in with the mouse and left click to put one down."),
            new SectionBuilder(Namepedia.Clerk, "After that, left click again to trigger the explosive.").
                WithAction(() => {
                    Debug.Log("Bombs unlocked, colliders disabled.");
                    // GameManager.Instance().stats.bombsUnlocked = true; disableCollider.enabled = false;
                }, DialogueActionTime.BEFORE_DISPLAYING_TEXT),
            new SectionBuilder(Namepedia.Mummy, "Why do you even have these?"),
            new SectionBuilder(Namepedia.Clerk, "It's my hobby. It's Antarctica. Nobody cares. Look, I just work here. Either get out or buy something.")
            );
        
        builder.WithSection(new SectionBuilder(Namepedia.Clerk, "Have a nice day, there's your receipt.", "aDone"));

        // builder.Build();
        
        return builder.GetSectionById(done ? "aDone" : "a");
    }

    public Transform SpeakingIcon() {
        return speakingIcon;
    }
}

public static class Namepedia {
    public static string Clerk = "Tim";
    public static string Mummy = "Steve";
    public static string Jen_Operator = "J.E.N. Operator";
    public static string Pay_Phone = "Phone";
    public static string Woman = "Amelia";

    public static string Pilot = "Pilot";
}